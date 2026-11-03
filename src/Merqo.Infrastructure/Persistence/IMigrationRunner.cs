using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Abstraction over EF Core's migration engine. Existing so that the safe-migration workflow
/// (backup → migrate → verify → recover) can be tested against a failing migrator without
/// hand-corrupting a real database.
/// </summary>
public interface IMigrationRunner
{
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);
    Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default);
    Task MigrateToAsync(string targetMigration, CancellationToken cancellationToken = default);
}

public sealed class EfMigrationRunner : IMigrationRunner
{
    private readonly string _databasePath;

    public EfMigrationRunner(string databasePath) => _databasePath = databasePath;

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = TahabilDbContextFactory.Create(_databasePath);
        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        return pending.ToList();
    }

    public async Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = TahabilDbContextFactory.Create(_databasePath);
        var applied = await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);
        return applied.ToList();
    }

    public async Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = TahabilDbContextFactory.Create(_databasePath);
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MigrateToAsync(string targetMigration, CancellationToken cancellationToken = default)
    {
        await using var context = TahabilDbContextFactory.Create(_databasePath);
        var migrator = context.Database.GetService<IMigrator>();
        await migrator.MigrateAsync(targetMigration, cancellationToken).ConfigureAwait(false);
    }
}
