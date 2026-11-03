namespace Merqo.Domain.Entities;

public enum AccountType
{
    Asset = 0,
    Liability = 1,
    Equity = 2,
    Revenue = 3,
    Expense = 4
}

public sealed class Account : Entity
{
    public string Code { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public AccountType AccountType { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum JournalSourceType
{
    Manual = 0,
    Sale = 1,
    SaleReturn = 2,
    Purchase = 3,
    PurchaseReturn = 4,
    CustomerPayment = 5,
    SupplierPayment = 6,
    Expense = 7,
    StockAdjustment = 8,
    Opening = 9,
    Reversal = 10
}

/// <summary>
/// Double-entry journal header. Posted entries are immutable; corrections are reversal entries that
/// point back at the original, so the audit trail is complete.
/// </summary>
public sealed class JournalEntry : Entity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
    public string Memo { get; set; } = string.Empty;
    public JournalSourceType SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceNumber { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfEntryId { get; set; }
    public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public sealed class JournalEntryLine : Entity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
    public string AccountCodeSnapshot { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Memo { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SupplierId { get; set; }
}

public sealed class ExpenseCategory : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    /// <summary>Chart-of-accounts code this category posts to (6000-series).</summary>
    public string AccountCode { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
}

public sealed class Expense : Entity
{
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public Guid ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public decimal Amount { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string MethodNameSnapshot { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? AttachmentPath { get; set; }
    public string? IdempotencyKey { get; set; }
    public bool IsVoided { get; set; }
    public DateTime? VoidedAtUtc { get; set; }
    public string? VoidReason { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
