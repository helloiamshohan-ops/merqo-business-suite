namespace Merqo.Domain.Entities;

public enum SaleStatus
{
    Completed = 0,
    PartiallyReturned = 1,
    Returned = 2,
    Voided = 3
}

public sealed class Sale : Entity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDateUtc { get; set; } = DateTime.UtcNow;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    public decimal Subtotal { get; set; }
    public decimal LineDiscountTotal { get; set; }
    public decimal InvoiceDiscount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal RoundingAdjustment { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }

    public string? Note { get; set; }
    /// <summary>Idempotency token: a repeated checkout with the same token must not create a second invoice.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public Guid? VoidedByUserId { get; set; }
    public DateTime? VoidedAtUtc { get; set; }
    public string? VoidReason { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
}

public sealed class SaleItem : Entity
{
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>Quantity expressed in the product's stock unit (dozen → 12 pcs).</summary>
    public decimal BaseQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal InvoiceDiscountShare { get; set; }
    public decimal TaxRatePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal NetRevenue { get; set; }
    /// <summary>Frozen weighted-average unit cost at the moment of sale.</summary>
    public decimal UnitCost { get; set; }
    public decimal CostAmount { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ReturnedQuantity { get; set; }
}

public sealed class SalePayment : Entity
{
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string MethodNameSnapshot { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? IdempotencyKey { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Reversal is a state change on the existing row; the row itself is never deleted.
    public bool IsReversed { get; set; }
    public DateTime? ReversedAtUtc { get; set; }
    public Guid? ReversedByUserId { get; set; }
    public string? ReversalReason { get; set; }
}

public sealed class SaleReturn : Entity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public bool IsVoided { get; set; }
    public DateTime? VoidedAtUtc { get; set; }
    public string? VoidReason { get; set; }
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }
    public DateTime ReturnDateUtc { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RestockValue { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public bool IsRefundToCash { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}

public sealed class SaleReturnItem : Entity
{
    public Guid SaleReturnId { get; set; }
    public SaleReturn? SaleReturn { get; set; }
    public Guid SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal UnitCost { get; set; }
    public decimal CostAmount { get; set; }
    public bool IsRestocked { get; set; } = true;
}
