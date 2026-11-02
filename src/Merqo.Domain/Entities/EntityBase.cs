namespace Merqo.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Base type for records that must never lose history. Archival replaces deletion for any entity
/// that is referenced by posted financial transactions.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }

    /// <summary>Optimistic concurrency token, incremented on every update.</summary>
    public int Version { get; set; }
}
