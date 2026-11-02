namespace Merqo.Domain.Entities;

/// <summary>
/// Customer master. Balances are never stored as a mutable field; they are derived from the ledger
/// so that the displayed due can always be reconciled against posted transactions.
/// </summary>
public sealed class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Note { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<CustomerLedgerEntry> Ledger { get; set; } = new List<CustomerLedgerEntry>();
}

public sealed class Supplier : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Note { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<SupplierLedgerEntry> Ledger { get; set; } = new List<SupplierLedgerEntry>();
}

public enum LedgerEntryType
{
    Opening = 0,
    Sale = 1,
    SaleReturn = 2,
    Payment = 3,
    Purchase = 4,
    PurchaseReturn = 5,
    Adjustment = 6,
    Advance = 7
}

/// <summary>
/// Append-only customer ledger. Positive <see cref="Debit"/> increases what the customer owes;
/// positive <see cref="Credit"/> reduces it. <see cref="BalanceAfter"/> is the running due.
/// </summary>
public sealed class CustomerLedgerEntry : Entity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
    public LedgerEntryType EntryType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public sealed class SupplierLedgerEntry : Entity
{
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
    public LedgerEntryType EntryType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public sealed class PaymentMethod : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public bool IsCash { get; set; }
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
    /// <summary>Chart-of-accounts code credited/debited when this method is used.</summary>
    public string CashAccountCode { get; set; } = "1000";
}
