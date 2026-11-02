using System.Globalization;
using Merqo.Application.Common;
using Merqo.Application.Contracts;
using Merqo.Domain.Common;
using Merqo.Domain.Entities;
using Merqo.Domain.Finance;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

/// <summary>
/// Records a purchase in one atomic transaction: header, items, stock receipts (which re-average the
/// moving weighted cost), supplier ledger, payment rows, balanced journal entry and audit record.
/// A purchase is never treated as an operating expense — it moves value into inventory.
/// </summary>
public sealed class PurchaseService
{
    private readonly TahabilDbContext _context;

    public PurchaseService(TahabilDbContext context) => _context = context;

    public async Task<OperationResult<PurchaseCreatedResult>> CreateAsync(
        CreatePurchaseRequest request,
        CurrentUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!user.Has(Permissions.PurchaseCreate))
            return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.PermissionDenied, "Missing permission purchases.create.");
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "An idempotency key is required.");
        if (request.Lines is null || request.Lines.Count == 0)
            return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "A purchase needs at least one line.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var duplicate = await _context.Purchases.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey, cancellationToken)
                .ConfigureAwait(false);
            if (duplicate is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.DuplicateSubmission, $"Purchase already recorded as {duplicate.PurchaseNumber}.");
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken)
                .ConfigureAwait(false);
            if (supplier is null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.NotFound, "Supplier not found.");
            }

            var paymentMethod = await _context.PaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentMethodId, cancellationToken)
                .ConfigureAwait(false);
            if (paymentMethod is null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.NotFound, "Payment method not found.");
            }

            var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken)
                .ConfigureAwait(false);

            var settings = await _context.BusinessSettings.AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? new BusinessSettings();

            decimal subtotal = 0m;
            decimal discountTotal = 0m;
            decimal taxTotal = 0m;
            var prepared = new List<(PurchaseLineRequest Request, Product Product, decimal BaseQuantity, decimal LineTotal)>();

            foreach (var line in request.Lines)
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.NotFound, $"Product {line.ProductId} not found.");
                }
                if (line.Quantity <= 0m || line.UnitFactorToBase <= 0m)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "Quantity and unit factor must be greater than zero.");
                }
                if (line.UnitCost < 0m || line.DiscountAmount < 0m || line.TaxRatePercent is < 0m or > 100m)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "Invalid cost, discount or tax rate.");
                }

                var gross = MoneyMath.Round(line.Quantity * line.UnitCost);
                if (line.DiscountAmount > gross)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult<PurchaseCreatedResult>.Fail(DomainErrorCodes.DiscountExceedsAmount, "Line discount cannot exceed the line amount.");
                }

                var taxable = gross - line.DiscountAmount;
                var tax = MoneyMath.Round(taxable * line.TaxRatePercent / 100m);
                var lineTotal = taxable + tax;

                subtotal += gross;
                discountTotal += line.DiscountAmount;
                taxTotal += tax;

                var baseQuantity = decimal.Round(line.Quantity * line.UnitFactorToBase, 4, MidpointRounding.AwayFromZero);
                prepared.Add((line, product, baseQuantity, lineTotal));
            }

            subtotal = MoneyMath.Round(subtotal);
            discountTotal = MoneyMath.Round(discountTotal);
            taxTotal = MoneyMath.Round(taxTotal);
            var grossTotal = MoneyMath.Round(subtotal - discountTotal + taxTotal);

            if (request.InvoiceDiscountAmount < 0m || request.InvoiceDiscountAmount > grossTotal)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<PurchaseCreatedResult>.Fail(DomainErrorCodes.DiscountExceedsAmount, "Invoice discount is out of range.");
            }

            var payable = MoneyMath.Round(grossTotal - request.InvoiceDiscountAmount);
            var paid = MoneyMath.Round(request.PaidAmount);
            if (paid < 0m || paid > payable)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.ValidationFailed, "Paid amount must be between zero and the purchase total.");
            }
            var due = MoneyMath.Round(payable - paid);

            var purchaseFormat = string.IsNullOrWhiteSpace(settings.InvoiceNumberFormat)
                ? "PUR-{YYYYMM}-{SEQ:5}"
                : "PUR-{YYYYMM}-{SEQ:5}";
            var purchaseNumber = await new DocumentNumberService(_context)
                .NextAsync(DocumentNumberService.PurchaseKey, purchaseFormat, DateTime.Now, cancellationToken)
                .ConfigureAwait(false);

            var purchase = new Purchase
            {
                PurchaseNumber = purchaseNumber,
                PurchaseDateUtc = DateTime.UtcNow,
                SupplierId = supplier.Id,
                SupplierNameSnapshot = supplier.Name,
                Status = PurchaseStatus.Completed,
                Subtotal = subtotal,
                DiscountTotal = MoneyMath.Round(discountTotal + request.InvoiceDiscountAmount),
                TaxTotal = taxTotal,
                GrandTotal = payable,
                PaidAmount = paid,
                DueAmount = due,
                Note = request.Note,
                IdempotencyKey = request.IdempotencyKey,
                CreatedByUserId = user.UserId
            };

            var stock = new StockService(_context);
            foreach (var item in prepared)
            {
                // Cost per base unit so that buying a carton of 12 at 1,200 stores 100 per piece.
                var baseUnitCost = decimal.Round(item.Request.UnitCost / item.Request.UnitFactorToBase, 4, MidpointRounding.AwayFromZero);
                await stock.ReceiveAsync(
                    item.Product.Id,
                    item.BaseQuantity,
                    baseUnitCost,
                    StockMovementType.Purchase,
                    "purchase",
                    purchase.Id,
                    purchaseNumber,
                    user,
                    cancellationToken).ConfigureAwait(false);

                purchase.Items.Add(new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = item.Product.Id,
                    ProductNameSnapshot = string.IsNullOrWhiteSpace(item.Product.NameBn) ? item.Product.Code : item.Product.NameBn,
                    UnitId = item.Product.UnitId,
                    Quantity = item.Request.Quantity,
                    BaseQuantity = item.BaseQuantity,
                    UnitCost = item.Request.UnitCost,
                    LineDiscount = item.Request.DiscountAmount,
                    TaxRatePercent = item.Request.TaxRatePercent,
                    TaxAmount = MoneyMath.Round((MoneyMath.Round(item.Request.Quantity * item.Request.UnitCost) - item.Request.DiscountAmount) * item.Request.TaxRatePercent / 100m),
                    LineTotal = item.LineTotal,
                    ReturnedQuantity = 0m
                });
            }

            _context.Purchases.Add(purchase);

            if (paid > 0m)
            {
                _context.PurchasePayments.Add(new PurchasePayment
                {
                    PurchaseId = purchase.Id,
                    PaymentMethodId = paymentMethod.Id,
                    MethodNameSnapshot = paymentMethod.NameBn,
                    Amount = paid,
                    PaidAtUtc = purchase.PurchaseDateUtc,
                    CreatedByUserId = user.UserId
                });
            }

            var ledger = new LedgerService(_context);
            await ledger.PostSupplierEntryAsync(
                supplier.Id,
                LedgerEntryType.Purchase,
                0m,
                payable,
                purchase.PurchaseDateUtc,
                "purchase",
                purchase.Id,
                purchaseNumber,
                user,
                cancellationToken).ConfigureAwait(false);

            if (paid > 0m)
            {
                await ledger.PostSupplierEntryAsync(
                    supplier.Id,
                    LedgerEntryType.Payment,
                    paid,
                    0m,
                    purchase.PurchaseDateUtc,
                    "purchase_payment",
                    purchase.Id,
                    purchaseNumber,
                    user,
                    cancellationToken).ConfigureAwait(false);
            }

            var journalLines = new List<JournalLineDraft>
            {
                new(AccountCodes.Inventory, payable, 0m, "মজুত বৃদ্ধি")
            };
            if (paid > 0m)
                journalLines.Add(new JournalLineDraft(paymentMethod.CashAccountCode, 0m, paid, "নগদ পরিশোধ"));
            if (due > 0m)
                journalLines.Add(new JournalLineDraft(AccountCodes.AccountsPayable, 0m, due, "সরবরাহকারীর পাওনা", null, supplier.Id));

            await new AccountingService(_context).PostAsync(
                JournalSourceType.Purchase,
                purchase.Id,
                purchaseNumber,
                $"ক্রয় {purchaseNumber}",
                purchase.PurchaseDateUtc,
                journalLines,
                user,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await new AuditService(_context).WriteAsync(
                "purchase.create",
                "purchase",
                purchase.Id.ToString(),
                $"purchase={purchaseNumber}; total={payable.ToString(CultureInfo.InvariantCulture)}; due={due.ToString(CultureInfo.InvariantCulture)}; items={purchase.Items.Count}",
                user.UserId,
                user.Username,
                true,
                cancellationToken).ConfigureAwait(false);

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return OperationResult<PurchaseCreatedResult>.Ok(new PurchaseCreatedResult(purchase.Id, purchaseNumber, payable, paid, due));
        }
        catch (DomainRuleViolationException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return OperationResult<PurchaseCreatedResult>.Fail(ex.Code, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return OperationResult<PurchaseCreatedResult>.Fail(ApplicationErrorCodes.InvalidState, ex.Message);
        }
    }
}
