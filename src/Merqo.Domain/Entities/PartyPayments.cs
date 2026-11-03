namespace Merqo.Domain.Entities;

/// <summary>
/// A standalone money receipt from a customer (due collection), independent of any single invoice.
/// Rows are never deleted: a mistake is corrected by reversing the payment and posting a new one.
/// </summary>
public sealed class CustomerPayment : Entity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string MethodNameSnapshot { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Note { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }

    public bool IsReversed { get; set; }
    public DateTime? ReversedAtUtc { get; set; }
    public Guid? ReversedByUserId { get; set; }
    public string? ReversalReason { get; set; }
}

/// <summary>A standalone payment to a supplier (due settlement), independent of any single purchase.</summary>
public sealed class SupplierPayment : Entity
{
    public string VoucherNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string MethodNameSnapshot { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Note { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }

    public bool IsReversed { get; set; }
    public DateTime? ReversedAtUtc { get; set; }
    public Guid? ReversedByUserId { get; set; }
    public string? ReversalReason { get; set; }
}
