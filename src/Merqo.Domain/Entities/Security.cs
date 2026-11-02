namespace Merqo.Domain.Entities;

public sealed class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}

public sealed class Role : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

public sealed class RolePermission : Entity
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
}

public sealed class UserRole : Entity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

/// <summary>
/// Append-only audit trail. Rows are hash-chained so a hand-edited database is detectable.
/// </summary>
public sealed class AuditLog : Entity
{
    /// <summary>Monotonic insertion ordinal; the hash chain is ordered by this, never by timestamp.</summary>
    public long Ordinal { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public bool Success { get; set; } = true;
    public string? CorrelationId { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public string RowHash { get; set; } = string.Empty;
}

public sealed class LoginAttempt : Entity
{
    public string Username { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? Reason { get; set; }
}

public sealed class DocumentSequence : Entity
{
    public string SequenceKey { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public long LastNumber { get; set; }
}

public sealed class BackupHistory : Entity
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string Kind { get; set; } = "manual";
    public bool IsVerified { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
