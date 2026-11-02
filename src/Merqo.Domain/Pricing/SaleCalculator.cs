using Merqo.Domain.Common;
using Merqo.Domain.Money;

namespace Merqo.Domain.Pricing;

public sealed record SaleLineInput(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitSellPrice,
    decimal LineDiscountAmount = 0m,
    decimal TaxRatePercent = 0m,
    decimal UnitCost = 0m);

public sealed record SaleRequest(
    IReadOnlyList<SaleLineInput> Lines,
    decimal InvoiceDiscountAmount = 0m,
    decimal PaidAmount = 0m,
    bool RoundGrandTotalToTaka = false);

public sealed record SaleLineResult(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitSellPrice,
    decimal GrossAmount,
    decimal LineDiscountAmount,
    decimal InvoiceDiscountShare,
    decimal TaxableAmount,
    decimal TaxAmount,
    decimal LineTotal,
    decimal NetRevenue,
    decimal CostAmount,
    decimal GrossProfit)
{
    public decimal EffectiveUnitPrice => Quantity == 0m ? 0m : MoneyMath.Round(NetRevenue / Quantity);
}

public sealed record SaleCalculation(
    IReadOnlyList<SaleLineResult> Lines,
    decimal Subtotal,
    decimal LineDiscountTotal,
    decimal InvoiceDiscountAmount,
    decimal TaxTotal,
    decimal GrossTotal,
    decimal RoundingAdjustment,
    decimal GrandTotal,
    decimal Payable,
    decimal Paid,
    decimal Due,
    decimal Change,
    decimal TotalCost,
    decimal GrossProfit)
{
    public decimal NetSales => MoneyMath.Round(Subtotal - LineDiscountTotal - InvoiceDiscountAmount);
    public int ItemCount => Lines.Count;
    public decimal QuantityTotal => Lines.Sum(l => l.Quantity);
}

/// <summary>
/// The single source of truth for sale arithmetic (POS and sales invoices alike).
/// No UI page may re-implement any part of this calculation.
/// </summary>
public static class SaleCalculator
{
    public static SaleCalculation Calculate(SaleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Lines is null || request.Lines.Count == 0)
            throw new DomainRuleViolationException(DomainErrorCodes.EmptySale, "A sale requires at least one line.");

        if (request.PaidAmount < 0m)
            throw new DomainRuleViolationException(DomainErrorCodes.NegativePayment, "Paid amount cannot be negative.");

        var staged = new List<Staged>(request.Lines.Count);
        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.QuantityNotPositive, "Line quantity must be greater than zero.");
            if (line.UnitSellPrice < 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.NegativePrice, "Unit price cannot be negative.");
            if (line.UnitCost < 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.NegativePrice, "Unit cost cannot be negative.");
            if (line.LineDiscountAmount < 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.NegativeDiscount, "Line discount cannot be negative.");
            if (line.TaxRatePercent < 0m || line.TaxRatePercent > 100m)
                throw new DomainRuleViolationException(DomainErrorCodes.InvalidTaxRate, "Tax rate must be between 0 and 100 percent.");

            var gross = MoneyMath.Round(line.Quantity * line.UnitSellPrice);
            if (line.LineDiscountAmount > gross)
                throw new DomainRuleViolationException(DomainErrorCodes.DiscountExceedsAmount, "Line discount cannot exceed the line gross amount.");

            var taxable = gross - line.LineDiscountAmount;
            var tax = MoneyMath.Round(taxable * line.TaxRatePercent / 100m);
            var lineTotal = taxable + tax;
            var cost = MoneyMath.Round(line.Quantity * line.UnitCost);

            staged.Add(new Staged(line, gross, taxable, tax, lineTotal, cost));
        }

        var subtotal = MoneyMath.Round(staged.Sum(s => s.Gross));
        var lineDiscountTotal = MoneyMath.Round(staged.Sum(s => s.Line.LineDiscountAmount));
        var preInvoiceTotal = MoneyMath.Round(staged.Sum(s => s.LineTotal));

        if (request.InvoiceDiscountAmount < 0m)
            throw new DomainRuleViolationException(DomainErrorCodes.NegativeDiscount, "Invoice discount cannot be negative.");
        if (request.InvoiceDiscountAmount > preInvoiceTotal)
            throw new DomainRuleViolationException(DomainErrorCodes.DiscountExceedsAmount, "Invoice discount cannot exceed the invoice total.");

        // Invoice-level discount is allocated pro-rata over line totals so that
        // per-product revenue reporting still reconciles to the invoice total.
        var shares = MoneyMath.Allocate(
            MoneyMath.Round(request.InvoiceDiscountAmount),
            staged.Select(s => s.LineTotal).ToArray());

        var results = new List<SaleLineResult>(staged.Count);
        for (int i = 0; i < staged.Count; i++)
        {
            var s = staged[i];
            var netRevenue = s.Taxable - shares[i];
            results.Add(new SaleLineResult(
                s.Line.ProductId,
                s.Line.Description,
                s.Line.Quantity,
                s.Line.UnitSellPrice,
                s.Gross,
                s.Line.LineDiscountAmount,
                shares[i],
                s.Taxable,
                s.Tax,
                s.LineTotal,
                netRevenue,
                s.Cost,
                netRevenue - s.Cost));
        }

        var taxTotal = MoneyMath.Round(staged.Sum(s => s.Tax));
        var grossTotal = preInvoiceTotal;
        var afterDiscount = MoneyMath.Round(grossTotal - request.InvoiceDiscountAmount);
        var grandTotal = request.RoundGrandTotalToTaka ? MoneyMath.RoundToTaka(afterDiscount) : afterDiscount;
        var roundingAdjustment = MoneyMath.Round(grandTotal - afterDiscount);
        var payable = grandTotal;

        var paid = MoneyMath.Round(request.PaidAmount);
        var change = 0m;
        if (paid > payable)
        {
            change = MoneyMath.Round(paid - payable);
            paid = payable;
        }

        var due = MoneyMath.Round(payable - paid);
        var totalCost = MoneyMath.Round(staged.Sum(s => s.Cost));

        return new SaleCalculation(
            results,
            subtotal,
            lineDiscountTotal,
            MoneyMath.Round(request.InvoiceDiscountAmount),
            taxTotal,
            grossTotal,
            roundingAdjustment,
            grandTotal,
            payable,
            paid,
            due,
            change,
            totalCost,
            MoneyMath.Round(results.Sum(r => r.GrossProfit)));
    }

    /// <summary>Resolves a discount expressed either as a percentage or as an absolute amount.</summary>
    public static decimal ResolveDiscount(decimal baseAmount, decimal? percent, decimal? amount)
    {
        if (baseAmount < 0m) baseAmount = 0m;
        decimal resolved = 0m;
        if (percent is > 0m) resolved += MoneyMath.Round(baseAmount * percent.Value / 100m);
        if (amount is > 0m) resolved += MoneyMath.Round(amount.Value);
        if (resolved < 0m) resolved = 0m;
        return resolved > baseAmount ? MoneyMath.Round(baseAmount) : MoneyMath.Round(resolved);
    }

    private sealed record Staged(SaleLineInput Line, decimal Gross, decimal Taxable, decimal Tax, decimal LineTotal, decimal Cost);
}
