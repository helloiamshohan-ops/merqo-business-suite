namespace Merqo.Domain.Entities;

public enum LicenseType
{
    Trial = 0,
    Lifetime = 1
}

public enum MachineBindingPolicy
{
    None = 0,
    Optional = 1,
    Required = 2
}

/// <summary>Local record of the licence installed on this machine.</summary>
public sealed class LicenseRecord : Entity
{
    public string LicenseId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessIdentity { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public MachineBindingPolicy BindingPolicy { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ActivatedAtUtc { get; set; } = DateTime.UtcNow;
    public int Serial { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string PublicKeyId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? SupersededAtUtc { get; set; }
}

public sealed class ActivationRecord : Entity
{
    public Guid LicenseRecordId { get; set; }
    public DateTime ActivatedAtUtc { get; set; } = DateTime.UtcNow;
    public string MachineFingerprintHash { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
    public string? WindowUserName { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Hash of the machine fingerprint (never the raw hardware identifiers) used for binding checks.
/// </summary>
public sealed class MachineBinding : Entity
{
    public Guid LicenseRecordId { get; set; }
    public string FingerprintHash { get; set; } = string.Empty;
    public string ComponentHashes { get; set; } = string.Empty;
    public DateTime FirstSeenUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public bool IsCurrent { get; set; } = true;
}

public sealed class LicenseRecoveryRecord : Entity
{
    public string OriginalLicenseId { get; set; } = string.Empty;
    public string ReplacementLicenseId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}
