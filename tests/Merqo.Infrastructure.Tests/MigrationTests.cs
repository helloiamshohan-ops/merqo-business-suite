using FluentAssertions;
using Merqo.Domain.Entities;
using Merqo.Infrastructure;
using Merqo.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Merqo.Infrastructure.Tests;

/// <summary>
/// Milestone 1: the schema is owned by EF migrations, migrations are applied through a safe workflow
/// (verified backup → migrate → verify → automatic restore), and business data survives both upgrades
/// and failures.
/// </summary>
public sealed class MigrationTests
{
    private static string NewRoot() => Path.Combine(Path.GetTempPath(), "tahabil-migration-tests", Guid.NewGuid().ToString("N"));

    private static async Task<int> CountApplicationTablesAsync(string databasePath)
    {
        await using var connection = new SqliteConnection(TahabilDbContextFactory.BuildConnectionString(databasePath));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name <> '__EFMigrationsHistory';";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    [Fact]
    public async Task Fresh_database_is_created_by_the_initial_migration()
    {
        var paths = new AppPaths(NewRoot());
        var report = await new DatabaseMigrator(paths).MigrateAsync();

        report.DatabaseExisted.Should().BeFalse();
        report.PendingBefore.Should().ContainSingle(m => m.EndsWith("InitialCreate", StringComparison.Ordinal));
        report.IntegrityOk.Should().BeTrue();
        report.PreMigrationBackupPath.Should().BeNull("there is nothing to protect in a brand-new database");

        (await CountApplicationTablesAsync(paths.DatabasePath)).Should().BeGreaterThanOrEqualTo(45);

        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        var applied = await context.Database.GetAppliedMigrationsAsync();
        applied.Should().ContainSingle(m => m.EndsWith("InitialCreate", StringComparison.Ordinal));
        (await SqliteDiagnostics.IntegrityOkAsync(context)).Should().BeTrue();
    }

    [Fact]
    public async Task Migrated_database_enforces_wal_full_sync_and_foreign_keys()
    {
        var paths = new AppPaths(NewRoot());
        await new DatabaseMigrator(paths).MigrateAsync();

        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        (await SqliteDiagnostics.JournalModeAsync(context)).Should().Be("wal");
        (await SqliteDiagnostics.SynchronousAsync(context)).Should().Be(2);
        (await SqliteDiagnostics.ForeignKeysAsync(context)).Should().Be(1);

        context.ProductBarcodes.Add(new ProductBarcode { ProductId = Guid.NewGuid(), Barcode = "MRG-1" });
        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>("foreign keys must be enforced on a migrated database");
    }

    [Fact]
    public async Task Second_run_reports_no_pending_migrations_and_changes_nothing()
    {
        var paths = new AppPaths(NewRoot());
        var first = await new DatabaseMigrator(paths).MigrateAsync();
        var second = await new DatabaseMigrator(paths).MigrateAsync();

        first.PendingBefore.Should().NotBeEmpty();
        second.DatabaseExisted.Should().BeTrue();
        second.PendingBefore.Should().BeEmpty();
        second.PreMigrationBackupPath.Should().BeNull("no schema work means no need to back up");
        second.IntegrityOk.Should().BeTrue();
    }

    [Fact]
    public async Task Rollback_to_zero_and_forward_again_is_a_supported_recovery_path()
    {
        var paths = new AppPaths(NewRoot());
        await new DatabaseMigrator(paths).MigrateAsync();
        var tableCountBefore = await CountApplicationTablesAsync(paths.DatabasePath);
        tableCountBefore.Should().BeGreaterThan(40);

        var runner = new EfMigrationRunner(paths.DatabasePath);
        await runner.MigrateToAsync("0");

        (await CountApplicationTablesAsync(paths.DatabasePath)).Should().Be(0, "the documented down-migration removes the schema");
        (await runner.GetAppliedMigrationsAsync()).Should().BeEmpty();

        var forward = await new DatabaseMigrator(paths).MigrateAsync();
        forward.PendingBefore.Should().NotBeEmpty();
        forward.IntegrityOk.Should().BeTrue();
        (await CountApplicationTablesAsync(paths.DatabasePath)).Should().Be(tableCountBefore);

        // The re-created schema is fully usable.
        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        var unit = await context.Units.FirstAsync(u => u.NameEn == "Piece");
        context.Products.Add(new Product { NameBn = "পরীক্ষা", Code = "MIG-1", UnitId = unit.Id });
        await context.SaveChangesAsync();
        (await context.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Pending_work_on_an_existing_database_creates_a_verified_backup_first()
    {
        var paths = new AppPaths(NewRoot());
        await new DatabaseMigrator(paths).MigrateAsync();

        var migrator = new DatabaseMigrator(paths, null, () => new FakeMigrationRunner(new[] { "20990101_Fake" }));
        var report = await migrator.MigrateAsync();

        report.DatabaseExisted.Should().BeTrue();
        report.PendingBefore.Should().Equal("20990101_Fake");
        report.PreMigrationBackupPath.Should().NotBeNull();

        var backup = new SqliteBackupService();
        File.Exists(report.PreMigrationBackupPath!).Should().BeTrue();
        (await backup.VerifyAsync(report.PreMigrationBackupPath!)).Should().BeTrue();
    }

    [Fact]
    public async Task Failed_migration_restores_the_previous_database_intact()
    {
        var paths = new AppPaths(NewRoot());
        await new DatabaseMigrator(paths).MigrateAsync();

        await using (var seed = TahabilDbContextFactory.Create(paths.DatabasePath))
        {
            var unit = await seed.Units.FirstAsync(u => u.NameEn == "Piece");
            seed.Products.Add(new Product { NameBn = "আগের পণ্য", Code = "KEEP-1", UnitId = unit.Id });
            await seed.SaveChangesAsync();
        }
        SqliteConnection.ClearAllPools();

        var failing = new DatabaseMigrator(paths, null, () => new FakeMigrationRunner(new[] { "20990101_Fake" }, failOnApply: true));
        var act = async () => await failing.MigrateAsync();

        var exception = (await act.Should().ThrowAsync<DatabaseMigrationException>()).Which;
        exception.RecoveredFromBackup.Should().BeTrue();
        exception.BackupPath.Should().NotBeNull();

        // Business data is exactly as it was before the failed migration.
        await using var verify = TahabilDbContextFactory.Create(paths.DatabasePath);
        (await verify.Products.CountAsync()).Should().Be(1);
        (await verify.Products.SingleAsync()).Code.Should().Be("KEEP-1");
        (await SqliteDiagnostics.IntegrityOkAsync(verify)).Should().BeTrue();
    }

    [Fact]
    public async Task Corrupt_backup_is_rejected_and_never_restored()
    {
        var root = NewRoot();
        var paths = new AppPaths(root);
        await new DatabaseMigrator(paths).MigrateAsync();

        var service = new SqliteBackupService();
        var corrupt = Path.Combine(paths.BackupsDirectory, "corrupt.db");
        await File.WriteAllTextAsync(corrupt, "this is not a database");

        (await service.VerifyAsync(corrupt)).Should().BeFalse();
        var act = async () => await service.RestoreAsync(corrupt, paths.DatabasePath);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // The live database is untouched by the rejected restore.
        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        (await SqliteDiagnostics.IntegrityOkAsync(context)).Should().BeTrue();
    }

    [Fact]
    public async Task Restore_replaces_the_database_with_the_backup_content()
    {
        var paths = new AppPaths(NewRoot());
        await new DatabaseMigrator(paths).MigrateAsync();

        var service = new SqliteBackupService();
        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        var unit = await context.Units.FirstAsync(u => u.NameEn == "Piece");
        context.Products.Add(new Product { NameBn = "ব্যাকআপে আছে", Code = "B-1", UnitId = unit.Id });
        await context.SaveChangesAsync();

        var backup = await service.CreateAsync(paths.DatabasePath, paths.BackupsDirectory, "manual");
        backup.Verified.Should().BeTrue();
        backup.Sha256.Should().HaveLength(64);

        context.Products.Add(new Product { NameBn = "ব্যাকআপের পরে", Code = "B-2", UnitId = unit.Id });
        await context.SaveChangesAsync();
        (await context.Products.CountAsync()).Should().Be(2);

        await service.RestoreAsync(backup.FilePath, paths.DatabasePath);

        await using var restored = TahabilDbContextFactory.Create(paths.DatabasePath);
        var codes = await restored.Products.Select(p => p.Code).ToListAsync();
        codes.Should().Equal("B-1");
        (await SqliteDiagnostics.IntegrityOkAsync(restored)).Should().BeTrue();
    }

    [Fact]
    public async Task Business_data_persists_across_a_full_application_restart()
    {
        var paths = new AppPaths(NewRoot());

        await new DatabaseInitializer(paths).InitializeAsync();
        await using (var first = TahabilDbContextFactory.Create(paths.DatabasePath))
        {
            var unit = await first.Units.FirstAsync(u => u.NameEn == "Piece");
            first.Customers.Add(new Customer { Name = "রেস্টার্ট ক্রেতা", Phone = "01900000000" });
            first.Products.Add(new Product { NameBn = "রেস্টার্ট পণ্য", Code = "R-1", UnitId = unit.Id });
            await first.SaveChangesAsync();
        }

        // Simulate closing the application completely: drop every pooled connection and reopen.
        SqliteConnection.ClearAllPools();
        var initializedAgain = await new DatabaseInitializer(paths).InitializeAsync();

        initializedAgain.IntegrityOk.Should().BeTrue();
        initializedAgain.SeededUnits.Should().BeFalse();

        await using var second = TahabilDbContextFactory.Create(paths.DatabasePath);
        (await second.Customers.CountAsync(c => c.Name == "রেস্টার্ট ক্রেতা")).Should().Be(1);
        (await second.Products.CountAsync(p => p.Code == "R-1")).Should().Be(1);
        (await second.BusinessSettings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Migration_history_and_schema_stay_consistent_after_repeated_startups()
    {
        var paths = new AppPaths(NewRoot());
        for (int i = 0; i < 3; i++)
        {
            var result = await new DatabaseInitializer(paths).InitializeAsync();
            result.IntegrityOk.Should().BeTrue();
        }

        await using var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        (await context.Database.GetAppliedMigrationsAsync()).Should().ContainSingle();
        (await context.Units.CountAsync()).Should().Be(10, "seeding must be idempotent");
        (await context.Accounts.CountAsync()).Should().Be(22, "chart of accounts must be seeded once");
        (await context.PaymentMethods.CountAsync()).Should().Be(5);
        (await context.ExpenseCategories.CountAsync()).Should().Be(6);
    }

    private sealed class FakeMigrationRunner : IMigrationRunner
    {
        private readonly IReadOnlyList<string> _pending;
        private readonly bool _failOnApply;

        public FakeMigrationRunner(IReadOnlyList<string> pending, bool failOnApply = false)
        {
            _pending = pending;
            _failOnApply = failOnApply;
        }

        public Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_pending);

        public Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        public Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken = default)
            => _failOnApply ? throw new InvalidOperationException("simulated migration failure") : Task.CompletedTask;

        public Task MigrateToAsync(string targetMigration, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
