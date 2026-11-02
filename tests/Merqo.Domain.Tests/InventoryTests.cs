using FluentAssertions;
using Merqo.Domain.Common;
using Merqo.Domain.Inventory;
using Xunit;

namespace Merqo.Domain.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void Negative_stock_is_blocked_by_default()
    {
        StockPolicy.CanIssue(5m, 5m, allowNegativeStock: false).Should().BeTrue();
        StockPolicy.CanIssue(5m, 6m, allowNegativeStock: false).Should().BeFalse();
        StockPolicy.CanIssue(0m, 1m, allowNegativeStock: true).Should().BeTrue();
    }

    [Fact]
    public void Insufficient_stock_raises_domain_error()
    {
        var act = () => StockPolicy.EnsureCanIssue(2m, 3m, false, "চাল");
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.InsufficientStock);
    }

    [Theory]
    [InlineData(0, 5, StockLevel.OutOfStock)]
    [InlineData(3, 5, StockLevel.Low)]
    [InlineData(9, 5, StockLevel.Healthy)]
    public void Stock_level_classification_is_correct(decimal qty, decimal min, StockLevel expected)
        => StockPolicy.Classify(qty, min).Should().Be(expected);

    [Fact]
    public void Weighted_average_blends_receipts()
    {
        // 10 units at 100 = 1000, plus 10 units at 120 = 1200 -> 2200 / 20 = 110
        WeightedAverageCost.NewAverage(10m, 100m, 10m, 120m).Should().Be(110m);
    }

    [Fact]
    public void First_receipt_into_empty_stock_uses_purchase_cost()
        => WeightedAverageCost.NewAverage(0m, 0m, 7m, 45.5m).Should().Be(45.5m);

    [Fact]
    public void Cost_of_issue_is_rounded_to_two_decimals()
        => WeightedAverageCost.CostOfIssue(3m, 33.3333m).Amount.Should().Be(100.00m);

    [Fact]
    public void Inventory_value_uses_average_cost()
        => WeightedAverageCost.InventoryValue(12m, 25.5m).Amount.Should().Be(306.00m);
}
