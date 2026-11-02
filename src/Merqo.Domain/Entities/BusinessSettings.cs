namespace Merqo.Domain.Entities;

/// <summary>Single-row table holding business identity and invoice defaults.</summary>
public sealed class BusinessSettings : Entity
{
    public string BusinessName { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? AddressLine { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public string CurrencyCode { get; set; } = "BDT";
    public string CurrencySymbol { get; set; } = "৳";
    public int DecimalPlaces { get; set; } = 2;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string InvoiceNumberFormat { get; set; } = "INV-{YYYYMM}-{SEQ:5}";
    public string InvoiceHeader { get; set; } = string.Empty;
    public string InvoiceFooter { get; set; } = "ধন্যবাদ, আবার আসবেন।";
    public decimal DefaultVatPercent { get; set; }
    public bool AllowNegativeStock { get; set; }
    public bool RoundGrandTotalToTaka { get; set; }
    public int ReceiptPaperWidthMm { get; set; } = 80;
    public int ReceiptCopies { get; set; } = 1;
    public string? ReceiptPrinterName { get; set; }
    public bool IsRasterReceipt { get; set; } = true;
    public string? BackupFolder { get; set; }
    public int AutomaticBackupRetentionCount { get; set; } = 30;
    public bool AutomaticBackupEnabled { get; set; } = true;
    public DateTime? LastBackupAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

/// <summary>Key/value table for UI-level settings that do not belong to the business profile.</summary>
public sealed class ApplicationSetting : Entity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
