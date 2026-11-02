using System.Security.Cryptography;
using System.Text;
using Merqo.Domain.Entities;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

/// <summary>
/// Append-only, hash-chained audit trail. Each row's hash covers the previous row's hash, so deleting or
/// silently editing an audit row is detectable by re-walking the chain.
/// </summary>
public sealed class AuditService
{
    private readonly TahabilDbContext _context;

    public AuditService(TahabilDbContext context) => _context = context;

    public async Task WriteAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? details = null,
        Guid? userId = null,
        string? username = null,
        bool success = true,
        CancellationToken cancellationToken = default)
    {
        var last = await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Ordinal)
            .Select(a => new { a.RowHash, a.Ordinal })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var previousHash = last?.RowHash ?? string.Empty;
        var ordinal = (last?.Ordinal ?? 0L) + 1L;

        var occurredAt = DateTime.UtcNow;
        var row = new AuditLog
        {
            Ordinal = ordinal,
            OccurredAtUtc = occurredAt,
            UserId = userId,
            Username = username,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Success = success,
            PreviousHash = previousHash
        };
        row.RowHash = ComputeHash(row);

        _context.AuditLogs.Add(row);
    }

    public static string ComputeHash(AuditLog row)
    {
        var payload = string.Join('|',
            row.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture),
            row.PreviousHash,
            row.OccurredAtUtc.ToString("O"),
            row.Username ?? string.Empty,
            row.Action,
            row.EntityType ?? string.Empty,
            row.EntityId ?? string.Empty,
            row.Details ?? string.Empty,
            row.Success ? "1" : "0");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    /// <summary>Re-walks the chain and returns the first row that does not match, or null when intact.</summary>
    public async Task<Guid?> FindFirstTamperedRowAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _context.AuditLogs.AsNoTracking()
            .OrderBy(a => a.Ordinal)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        string previous = string.Empty;
        foreach (var row in rows)
        {
            if (row.PreviousHash != previous) return row.Id;
            if (ComputeHash(row) != row.RowHash) return row.Id;
            previous = row.RowHash;
        }
        return null;
    }
}
