using Merqo.Domain.Common;

namespace Merqo.Domain.Inventory;

/// <summary>
/// Stock issuance rules. By default a sale may never take stock below zero;
/// overriding this requires the explicit <c>inventory.allow_negative_stock</c> business setting
/// plus the <c>stock.negative.override</c> permission, and every override is audited.
/// </summary>
public static class StockPolicy
{
    public static bool CanIssue(decimal available, decimal requested, bool allowNegativeStock)
        => allowNegativeStock || requested <= available;

    public static void EnsureCanIssue(decimal available, decimal requested, bool allowNegativeStock, string productLabel)
    {
        if (requested <= 0m)
            throw new DomainRuleViolationException(DomainErrorCodes.QuantityNotPositive, "Issued quantity must be greater than zero.");
        if (!CanIssue(available, requested, allowNegativeStock))
        {
            throw new DomainRuleViolationException(
                DomainErrorCodes.InsufficientStock,
                $"Insufficient stock for '{productLabel}': available {available}, requested {requested}.");
        }
    }

    /// <summary>Classifies a stock level for dashboards and alerts.</summary>
    public static StockLevel Classify(decimal quantity, decimal minimumQuantity)
    {
        if (quantity <= 0m) return StockLevel.OutOfStock;
        if (quantity <= minimumQuantity) return StockLevel.Low;
        return StockLevel.Healthy;
    }
}

public enum StockLevel
{
    Healthy = 0,
    Low = 1,
    OutOfStock = 2
}
