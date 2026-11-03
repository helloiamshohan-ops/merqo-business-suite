namespace Merqo.Domain.Entities;

public enum StockMovementType
{
    Opening = 0,
    Purchase = 1,
    Sale = 2,
    SaleReturn = 3,
    PurchaseReturn = 4,
    AdjustmentIncrease = 5,
    AdjustmentDecrease = 6,
    Reversal = 7,
    Transfer = 8
}

/// <summary>Current stock position per product. Always paired with an immutable movement row.</summary>
public sealed class StockBalance : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public int Version { get; set; }
}

/// <summary>
/// Immutable stock ledger. Every quantity that moves in or out of the shop is one row here, with the
/// source document and the cost that was actually used, so valuation and COGS are reproducible.
/// </summary>
public sealed class StockMovement : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTime MovedAtUtc { get; set; } = DateTime.UtcNow;
    public StockMovementType MovementType { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal UnitCost { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public enum StockAdjustmentType
{
    Increase = 0,
    Decrease = 1,
    Reconciliation = 2
}

public sealed class StockAdjustment : Entity
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public bool IsVoided { get; set; }
    public DateTime? VoidedAtUtc { get; set; }
    public string? VoidReason { get; set; }
    public DateTime AdjustmentDateUtc { get; set; } = DateTime.UtcNow;
    public StockAdjustmentType AdjustmentType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalValueImpact { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}

public sealed class StockAdjustmentItem : Entity
{
    public Guid StockAdjustmentId { get; set; }
    public StockAdjustment? StockAdjustment { get; set; }
    public Guid ProductId { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal AdjustedQuantity { get; set; }
    public decimal FinalQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
