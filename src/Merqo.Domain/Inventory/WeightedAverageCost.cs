using Merqo.Domain.Finance;

namespace Merqo.Domain.Inventory;

/// <summary>
/// Inventory valuation uses moving weighted average cost. Every stock-in recalculates the average;
/// every stock-out freezes the average at the moment of the movement so that historical cost of goods
/// sold never changes retroactively when later purchase prices differ.
/// </summary>
public static class WeightedAverageCost
{
    /// <summary>Returns the new average unit cost after receiving <paramref name="inQuantity"/> at <paramref name="inUnitCost"/>.</summary>
    public static decimal NewAverage(decimal currentQuantity, decimal currentAverageCost, decimal inQuantity, decimal inUnitCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(inQuantity);
        var existingQty = currentQuantity > 0m ? currentQuantity : 0m;
        var existingValue = currentQuantity > 0m ? currentQuantity * currentAverageCost : 0m;
        var incomingValue = inQuantity * inUnitCost;
        var newQuantity = existingQty + inQuantity;
        if (newQuantity <= 0m) return 0m;
        // Keep 4 decimals internally so repeated receipts do not drift, round to 2 decimals for posting.
        return decimal.Round((existingValue + incomingValue) / newQuantity, 4, MidpointRounding.AwayFromZero);
    }

    /// <summary>Cost of goods sold for an issue, using the frozen average cost of the movement.</summary>
    public static Money CostOfIssue(decimal issueQuantity, decimal averageCost)
        => new(MoneyMath.Round(issueQuantity * averageCost));

    /// <summary>Total inventory value at the given average cost.</summary>
    public static Money InventoryValue(decimal quantity, decimal averageCost)
        => new(MoneyMath.Round(quantity * averageCost));
}
