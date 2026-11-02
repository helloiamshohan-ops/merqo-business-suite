namespace Merqo.Domain.Entities;

public enum PurchaseStatus
{
    Completed = 0,
    PartiallyReturned = 1,
    Returned = 2,
    Voided = 3
}

public sealed class Purchase : Entity
{
    public string PurchaseNumber { get; set; } = string.Empty;
    public DateTime PurchaseDateUtc { get; set; } = DateTime.UtcNow;
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string? SupplierNameSnapshot { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Completed;

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }

    public string? Note { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? VoidedAtUtc { get; set; }
    public string? VoidReason { get; set; }

    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    public ICollection<PurchasePayment> Payments { get; set; } = new List<PurchasePayment>();
}

public sealed class PurchaseItem : Entity
{
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal TaxRatePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReturnedQuantity { get; set; }
}

public sealed class PurchasePayment : Entity
{
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string MethodNameSnapshot { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public sealed class PurchaseReturn : Entity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public DateTime ReturnDateUtc { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}

public sealed class PurchaseReturnItem : Entity
{
    public Guid PurchaseReturnId { get; set; }
    public PurchaseReturn? PurchaseReturn { get; set; }
    public Guid PurchaseItemId { get; set; }
    public PurchaseItem? PurchaseItem { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Amount { get; set; }
}
