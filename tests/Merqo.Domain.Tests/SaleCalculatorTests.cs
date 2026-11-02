using FluentAssertions;
using Merqo.Domain.Common;
using Merqo.Domain.Pricing;
using Xunit;

namespace Merqo.Domain.Tests;

public sealed class SaleCalculatorTests
{
    private static SaleLineInput Line(decimal qty = 2m, decimal price = 100m, decimal discount = 0m, decimal tax = 0m, decimal cost = 70m)
        => new(Guid.NewGuid(), "পণ্য", qty, price, discount, tax, cost);

    [Fact]
    public void Simple_cash_sale_totals_are_correct()
    {
        var result = SaleCalculator.Calculate(new SaleRequest(new[] { Line() }, PaidAmount: 200m));

        result.Subtotal.Should().Be(200m);
        result.GrandTotal.Should().Be(200m);
        result.Paid.Should().Be(200m);
        result.Due.Should().Be(0m);
        result.Change.Should().Be(0m);
        result.TotalCost.Should().Be(140m);
        result.GrossProfit.Should().Be(60m);
    }

    [Fact]
    public void Line_discount_reduces_taxable_amount_and_payable()
    {
        var result = SaleCalculator.Calculate(new SaleRequest(new[] { Line(discount: 20m, tax: 5m) }, PaidAmount: 189m));

        result.LineDiscountTotal.Should().Be(20m);
        result.TaxTotal.Should().Be(9m);
        result.GrandTotal.Should().Be(189m);
        result.NetSales.Should().Be(180m);
    }

    [Fact]
    public void Invoice_discount_is_allocated_pro_rata_and_reconciles()
    {
        var lines = new[] { Line(1m, 100m), Line(1m, 200m), Line(1m, 300m) };
        var result = SaleCalculator.Calculate(new SaleRequest(lines, InvoiceDiscountAmount: 60m, PaidAmount: 540m));

        result.InvoiceDiscountAmount.Should().Be(60m);
        result.Lines.Sum(l => l.InvoiceDiscountShare).Should().Be(60m);
        result.Lines.Select(l => l.InvoiceDiscountShare).Should().Equal(10m, 20m, 30m);
        result.GrandTotal.Should().Be(540m);
        result.NetSales.Should().Be(540m);
    }

    [Fact]
    public void Partial_payment_records_due_amount()
    {
        var result = SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 3m, price: 50m) }, PaidAmount: 100m));

        result.GrandTotal.Should().Be(150m);
        result.Paid.Should().Be(100m);
        result.Due.Should().Be(50m);
    }

    [Fact]
    public void Overpayment_produces_change_and_never_negative_due()
    {
        var result = SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 1m, price: 199.50m) }, PaidAmount: 200m));

        result.Change.Should().Be(0.50m);
        result.Paid.Should().Be(199.50m);
        result.Due.Should().Be(0m);
    }

    [Fact]
    public void Cash_rounding_to_taka_is_recorded_as_adjustment()
    {
        var result = SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 1m, price: 100.40m) }, PaidAmount: 100m, RoundGrandTotalToTaka: true));

        result.GrandTotal.Should().Be(100m);
        result.RoundingAdjustment.Should().Be(-0.40m);
        result.Due.Should().Be(0m);
    }

    [Fact]
    public void Empty_sale_is_rejected()
    {
        var act = () => SaleCalculator.Calculate(new SaleRequest(Array.Empty<SaleLineInput>()));
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.EmptySale);
    }

    [Fact]
    public void Zero_quantity_is_rejected()
    {
        var act = () => SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 0m) }));
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.QuantityNotPositive);
    }

    [Fact]
    public void Discount_above_line_total_is_rejected()
    {
        var act = () => SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 1m, price: 100m, discount: 101m) }));
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.DiscountExceedsAmount);
    }

    [Fact]
    public void Invoice_discount_above_total_is_rejected()
    {
        var act = () => SaleCalculator.Calculate(new SaleRequest(new[] { Line(qty: 1m, price: 100m) }, InvoiceDiscountAmount: 150m));
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.DiscountExceedsAmount);
    }

    [Fact]
    public void Negative_payment_is_rejected()
    {
        var act = () => SaleCalculator.Calculate(new SaleRequest(new[] { Line() }, PaidAmount: -1m));
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.NegativePayment);
    }

    [Fact]
    public void Percent_and_amount_discounts_can_be_combined_within_bounds()
    {
        SaleCalculator.ResolveDiscount(1000m, 10m, 50m).Should().Be(150m);
        SaleCalculator.ResolveDiscount(1000m, 200m, null).Should().Be(1000m);
        SaleCalculator.ResolveDiscount(0m, 10m, 0m).Should().Be(0m);
    }
}
