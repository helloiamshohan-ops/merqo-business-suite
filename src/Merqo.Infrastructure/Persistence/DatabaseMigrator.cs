using Merqo.Infrastructure.Logging;

namespace Merqo.Infrastructure.Persistence;

public sealed record MigrationReport(
    bool DatabaseExisted,
    IReadOnlyList<string> PendingBefore,
    IReadOnlyList<string> AppliedNow,
    string? PreMigrationBackupPath,
    bool IntegrityOk,
    bool RecoveredFromBackup);

/// <summary>Raised when a migration fails. <see cref="RecoveredFromBackup"/> tells the caller whether the
/// previous database was restored intact (so business data is safe but the app must not continue).</summary>
public sealed class DatabaseMigrationException : Exception
{
    public DatabaseMigrationException(string message, bool recoveredFromBackup, string? backupPath, Exception? inner = null)
        : base(message, inner)
    {
        RecoveredFromBackup = recoveredFromBackup;
        BackupPath = backupPath;
    }

    public bool RecoveredFromBackup { get; }
    public string? BackupPath { get; }
}

/// <summary>
/// Applies schema migrations safely:
///   1. inspect pending migrations;
///   2. if an existing database has work to do, take a **verified** backup before touching it;
///   3. apply migrations (SQLite DDL is transactional, so a single migration is all-or-nothing);
///   4. verify integrity afterwards;
///   5. if anything fails and a backup exists, restore it automatically so no business data is lost.
/// </summary>
public sealed class DatabaseMigrator
{
    private readonly AppPaths _paths;
    private readonly IAppLogger? _logger;
    private readonly Func<IMigrationRunner> _runnerFactory;
    private readonly SqliteBackupService _backup;

    public DatabaseMigrator(AppPaths paths, IAppLogger? logger = null)
        : this(paths, logger, () => new EfMigrationRunner(paths.DatabasePath))
    {
    }

    public DatabaseMigrator(AppPaths paths, IAppLogger? logger, Func<IMigrationRunner> runnerFactory)
    {
        _paths = paths;
        _logger = logger;
        _runnerFactory = runnerFactory;
        _backup = new SqliteBackupService();
    }

    public async Task<MigrationReport> MigrateAsync(CancellationToken cancellationToken = default)
    {
        _paths.EnsureCreated();

        var existed = File.Exists(_paths.DatabasePath) && new FileInfo(_paths.DatabasePath).Length > 0;
        var runner = _runnerFactory();
        var pending = await runner.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        string? backupPath = null;

        if (existed && pending.Count > 0)
        {
            var backup = await _backup.CreateAsync(_paths.DatabasePath, _paths.BackupsDirectory, "pre-migration", cancellationToken)
                .ConfigureAwait(false);
            if (!backup.Verified)
                throw new DatabaseMigrationException(
                    "Refusing to migrate: the pre-migration backup failed verification.", false, backup.FilePath);
            backupPath = backup.FilePath;
            _logger?.Info(LogCategories.Database, "Pre-migration backup created", new Dictionary<string, object?>
            {
                ["path"] = backupPath,
                ["sha256"] = backup.Sha256,
                ["pending"] = pending.Count
            });
        }

        try
        {
            await runner.ApplyPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (backupPath is not null)
            {
                try
                {
                    await _backup.RestoreAsync(backupPath, _paths.DatabasePath, cancellationToken).ConfigureAwait(false);
                    _logger?.Error(LogCategories.Database, "Migration failed; database restored from pre-migration backup", ex,
                        new Dictionary<string, object?> { ["backup"] = backupPath });
                    throw new DatabaseMigrationException(
                        "Database migration failed. The previous database was restored from the automatic backup, so no business data was lost.",
                        true,
                        backupPath,
                        ex);
                }
                catch (DatabaseMigrationException)
                {
                    throw;
                }
                catch (Exception restoreEx)
                {
                    _logger?.Critical(LogCategories.Database, "Migration failed AND restore failed", restoreEx,
                        new Dictionary<string, object?> { ["backup"] = backupPath });
                    throw new DatabaseMigrationException(
                        "Database migration failed and the automatic restore also failed. Stop using the application and restore the backup manually.",
                        false,
                        backupPath,
                        ex);
                }
            }

            _logger?.Error(LogCategories.Database, "Migration failed on a new database", ex);
            throw new DatabaseMigrationException("Database initialisation failed.", false, null, ex);
        }

        var integrityOk = await VerifyIntegrityAsync(cancellationToken).ConfigureAwait(false);
        var applied = await runner.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);

        _logger?.Info(LogCategories.Database, "Migration completed", new Dictionary<string, object?>
        {
            ["pendingBefore"] = pending.Count,
            ["appliedTotal"] = applied.Count,
            ["integrityOk"] = integrityOk,
            ["backup"] = backupPath
        });

        return new MigrationReport(existed, pending, pending, backupPath, integrityOk, false);
    }

    public async Task<bool> VerifyIntegrityAsync(CancellationToken cancellationToken = default)
    {
        await using var context = TahabilDbContextFactory.Create(_paths.DatabasePath);
        return await SqliteDiagnostics.IntegrityOkAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
