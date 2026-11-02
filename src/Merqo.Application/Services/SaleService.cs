using System.Globalization;
using Merqo.Application.Common;
using Merqo.Application.Contracts;
using Merqo.Domain.Common;
using Merqo.Domain.Entities;
using Merqo.Domain.Finance;
using Merqo.Domain.Pricing;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

/// <summary>
/// Creates a completed sale as ONE atomic transaction: invoice, items, stock issues with frozen costs,
/// payment rows, customer ledger, balanced journal entry and audit record. If any part fails, nothing is
/// committed. Printing is deliberately not part of this method.
/// </summary>
public sealed class SaleService
{
    private readonly TahabilDbContext _context;

    public SaleService(TahabilDbContext context) => _context = context;

    public async Task<OperationResult<SaleCreatedResult>> CreateAsync(
        CreateSaleRequest request,
        CurrentUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!user.Has(Permissions.SaleCreate))
            return OperationResult<SaleCreatedResult>.Fail(ApplicationErrorCodes.PermissionDenied, "Missing permission sales.create.");
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return OperationResult<SaleCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "An idempotency key is required.");
        if (request.Lines is null || request.Lines.Count == 0)
            return OperationResult<SaleCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "A sale needs at least one line.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var duplicate = await _context.Sales.AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdempotencyKey == request.IdempotencyKey, cancellationToken)
                .ConfigureAwait(false);
            if (duplicate is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<SaleCreatedResult>.Fail(
                    ApplicationErrorCodes.DuplicateSubmission,
                    $"Sale already committed as {duplicate.InvoiceNumber}.");
            }

            var settings = await _context.BusinessSettings.AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? new BusinessSettings();

            var paymentMethod = await _context.PaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentMethodId && !p.IsArchived, cancellationToken)
                .ConfigureAwait(false);
            if (paymentMethod is null)
                return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.NotFound, "Payment method not found.", cancellationToken).ConfigureAwait(false);

            Customer? customer = null;
            if (request.CustomerId is { } customerId)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken)
                    .ConfigureAwait(false);
                if (customer is null)
                    return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.NotFound, "Customer not found.", cancellationToken).ConfigureAwait(false);
            }

            var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken)
                .ConfigureAwait(false);

            var lineInputs = new List<SaleLineInput>(request.Lines.Count);
            var bases = new List<decimal>(request.Lines.Count);
            foreach (var line in request.Lines)
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                    return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.NotFound, $"Product {line.ProductId} not found.", cancellationToken).ConfigureAwait(false);
                if (line.Quantity <= 0m)
                    return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.ValidationFailed, "Quantity must be greater than zero.", cancellationToken).ConfigureAwait(false);
                if (line.UnitFactorToBase <= 0m)
                    return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.ValidationFailed, "Unit conversion factor must be positive.", cancellationToken).ConfigureAwait(false);

                var baseQuantity = decimal.Round(line.Quantity * line.UnitFactorToBase, 4, MidpointRounding.AwayFromZero);
                bases.Add(baseQuantity);
                lineInputs.Add(new SaleLineInput(
                    product.Id,
                    string.IsNullOrWhiteSpace(product.NameBn) ? product.Code : product.NameBn,
                    line.Quantity,
                    line.UnitPrice,
                    line.DiscountAmount,
                    line.TaxRatePercent,
                    0m));
            }

            SaleCalculation calculation;
            try
            {
                calculation = SaleCalculator.Calculate(new SaleRequest(
                    lineInputs,
                    request.InvoiceDiscountAmount,
                    request.PaidAmount,
                    request.RoundGrandTotalToTaka ?? settings.RoundGrandTotalToTaka));
            }
            catch (DomainRuleViolationException ex)
            {
                return await AbortAsync<SaleCreatedResult>(transaction, ex.Code, ex.Message, cancellationToken).ConfigureAwait(false);
            }

            if (calculation.Due > 0m && customer is null)
                return await AbortAsync<SaleCreatedResult>(transaction, ApplicationErrorCodes.ValidationFailed, "A due amount requires a customer.", cancellationToken).ConfigureAwait(false);

            var invoiceFormat = string.IsNullOrWhiteSpace(settings.InvoiceNumberFormat) ? "INV-{YYYYMM}-{SEQ:5}" : settings.InvoiceNumberFormat;
            var invoiceNumber = await new DocumentNumberService(_context)
                .NextAsync(DocumentNumberService.SaleKey, invoiceFormat, DateTime.Now, cancellationToken)
                .ConfigureAwait(false);

            var sale = new Sale
            {
                InvoiceNumber = invoiceNumber,
                SaleDateUtc = DateTime.UtcNow,
                CustomerId = customer?.Id,
                CustomerNameSnapshot = customer?.Name,
                Status = SaleStatus.Completed,
                Subtotal = calculation.Subtotal,
                LineDiscountTotal = calculation.LineDiscountTotal,
                InvoiceDiscount = calculation.InvoiceDiscountAmount,
                TaxTotal = calculation.TaxTotal,
                RoundingAdjustment = calculation.RoundingAdjustment,
                GrandTotal = calculation.GrandTotal,
                PaidAmount = calculation.Paid,
                DueAmount = calculation.Due,
                ChangeAmount = calculation.Change,
                Note = request.Note,
                IdempotencyKey = request.IdempotencyKey,
                CreatedByUserId = user.UserId
            };

            var stock = new StockService(_context);
            decimal totalCost = 0m;
            decimal netRevenue = 0m;

            for (int i = 0; i < calculation.Lines.Count; i++)
            {
                var line = calculation.Lines[i];
                var product = products[line.ProductId];

                // Stock is issued at the frozen average cost; that cost is authoritative for COGS.
                var issue = await stock.IssueAsync(
                    product.Id,
                    bases[i],
                    StockMovementType.Sale,
                    "sale",
                    sale.Id,
                    invoiceNumber,
                    settings.AllowNegativeStock,
                    user,
                    product.NameBn,
                    cancellationToken).ConfigureAwait(false);

                totalCost += issue.CostAmount;
                netRevenue += line.NetRevenue;

                sale.Items.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    ProductNameSnapshot = line.Description,
                    UnitId = product.UnitId,
                    Quantity = line.Quantity,
                    BaseQuantity = bases[i],
                    UnitPrice = line.UnitSellPrice,
                    GrossAmount = line.GrossAmount,
                    LineDiscount = line.LineDiscountAmount,
                    InvoiceDiscountShare = line.InvoiceDiscountShare,
                    TaxRatePercent = request.Lines[i].TaxRatePercent,
                    TaxAmount = line.TaxAmount,
                    LineTotal = line.LineTotal,
                    NetRevenue = line.NetRevenue,
                    UnitCost = issue.FrozenUnitCost,
                    CostAmount = issue.CostAmount,
                    GrossProfit = MoneyMath.Round(line.NetRevenue - issue.CostAmount),
                    ReturnedQuantity = 0m
                });
            }

            totalCost = MoneyMath.Round(totalCost);
            netRevenue = MoneyMath.Round(netRevenue);
            sale.TotalCost = totalCost;
            sale.GrossProfit = MoneyMath.Round(netRevenue - totalCost);

            _context.Sales.Add(sale);

            if (calculation.Paid > 0m)
            {
                _context.SalePayments.Add(new SalePayment
                {
                    SaleId = sale.Id,
                    PaymentMethodId = paymentMethod.Id,
                    MethodNameSnapshot = paymentMethod.NameBn,
                    Amount = calculation.Paid,
                    PaidAtUtc = sale.SaleDateUtc,
                    CreatedByUserId = user.UserId
                });
            }

            var ledger = new LedgerService(_context);
            if (customer is not null && calculation.Due > 0m)
            {
                await ledger.PostCustomerEntryAsync(
                    customer.Id,
                    LedgerEntryType.Sale,
                    calculation.Due,
                    0m,
                    sale.SaleDateUtc,
                    "sale",
                    sale.Id,
                    invoiceNumber,
                    user,
                    cancellationToken).ConfigureAwait(false);
            }

            var journalLines = new List<JournalLineDraft>();

            if (calculation.Paid > 0m)
                journalLines.Add(new JournalLineDraft(paymentMethod.CashAccountCode, calculation.Paid, 0m, "নগদ গ্রহণ"));
            if (calculation.Due > 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.AccountsReceivable, calculation.Due, 0m, "ক্রেতার বাকি", customer?.Id, null));
            if (netRevenue != 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.SalesRevenue, 0m, netRevenue, "বিক্রয়"));
            if (calculation.TaxTotal > 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.VatPayable, 0m, calculation.TaxTotal, "ভ্যাট"));
            if (calculation.RoundingAdjustment > 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.RoundingDifference, 0m, calculation.RoundingAdjustment, "রাউন্ডিং"));
            if (calculation.RoundingAdjustment < 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.RoundingDifference, -calculation.RoundingAdjustment, 0m, "রাউন্ডিং"));
            if (totalCost > 0m)
            {
                journalLines.Add(new JournalLineDraft(AccountCodes.CostOfGoodsSold, totalCost, 0m, "বিক্রিত পণ্যের ব্যয়"));
                journalLines.Add(new JournalLineDraft(AccountCodes.Inventory, 0m, totalCost, "মজুত হ্রাস"));
            }

            await new AccountingService(_context).PostAsync(
                JournalSourceType.Sale,
                sale.Id,
                invoiceNumber,
                $"বিক্রয় {invoiceNumber}",
                sale.SaleDateUtc,
                journalLines,
                user,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await new AuditService(_context).WriteAsync(
                "sale.create",
                "sale",
                sale.Id.ToString(),
                $"invoice={invoiceNumber}; total={calculation.GrandTotal.ToString(CultureInfo.InvariantCulture)}; due={calculation.Due.ToString(CultureInfo.InvariantCulture)}; items={sale.Items.Count}",
                user.UserId,
                user.Username,
                true,
                cancellationToken).ConfigureAwait(false);

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return OperationResult<SaleCreatedResult>.Ok(new SaleCreatedResult(
                sale.Id,
                invoiceNumber,
                calculation.GrandTotal,
                calculation.Paid,
                calculation.Due,
                calculation.Change,
                netRevenue,
                totalCost,
                sale.GrossProfit));
        }
        catch (DomainRuleViolationException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return OperationResult<SaleCreatedResult>.Fail(ex.Code, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return OperationResult<SaleCreatedResult>.Fail(ApplicationErrorCodes.InvalidState, ex.Message);
        }
    }

    private static async Task<OperationResult<T>> AbortAsync<T>(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, string code, string message, CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult<T>.Fail(code, message);
    }
}
