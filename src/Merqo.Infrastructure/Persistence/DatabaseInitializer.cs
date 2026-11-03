using Merqo.Domain.Accounting;
using Merqo.Domain.Entities;
using Merqo.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Infrastructure.Persistence;

public sealed record DatabaseInitializationResult(bool Created, bool SeededUnits, bool IntegrityOk, IReadOnlyList<string> Messages);

/// <summary>
/// Creates or migrates the local database and seeds only system-level reference data
/// (units of measure and the empty business-profile row). No sample business transactions are ever seeded.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly AppPaths _paths;
    private readonly IAppLogger? _logger;

    public DatabaseInitializer(AppPaths paths, IAppLogger? logger = null)
    {
        _paths = paths;
        _logger = logger;
    }

    public async Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        _paths.EnsureCreated();
        var existedBefore = File.Exists(_paths.DatabasePath);

        // Schema is owned exclusively by EF Core migrations, applied through the safe migration workflow
        // (verified pre-migration backup + automatic restore on failure). EnsureCreated is never used,
        // because a database created that way has no migration history and cannot be upgraded.
        var migration = await new DatabaseMigrator(_paths, _logger).MigrateAsync(cancellationToken).ConfigureAwait(false);
        _ = migration; // report is logged by the migrator; seeding follows below

        await using var context = TahabilDbContextFactory.Create(_paths.DatabasePath);

        var seededUnits = await SeedUnitsAsync(context, cancellationToken).ConfigureAwait(false);
        await SeedBusinessSettingsAsync(context, cancellationToken).ConfigureAwait(false);
        await SeedChartOfAccountsAsync(context, cancellationToken).ConfigureAwait(false);
        await SeedPaymentMethodsAsync(context, cancellationToken).ConfigureAwait(false);
        await SeedExpenseCategoriesAsync(context, cancellationToken).ConfigureAwait(false);

        var integrityOk = await CheckIntegrityAsync(context, cancellationToken).ConfigureAwait(false);
        var messages = new List<string>
        {
            existedBefore ? "database.opened" : "database.created",
            seededUnits ? "units.seeded" : "units.present",
            integrityOk ? "integrity.ok" : "integrity.failed"
        };

        return new DatabaseInitializationResult(!existedBefore, seededUnits, integrityOk, messages);
    }

    public static Task<bool> CheckIntegrityAsync(TahabilDbContext context, CancellationToken cancellationToken = default)
        => SqliteDiagnostics.IntegrityOkAsync(context, cancellationToken);

    private static async Task<bool> SeedBusinessSettingsAsync(TahabilDbContext context, CancellationToken cancellationToken)
    {
        if (await context.BusinessSettings.AnyAsync(cancellationToken).ConfigureAwait(false)) return false;

        context.BusinessSettings.Add(new BusinessSettings
        {
            BusinessName = string.Empty,
            CurrencyCode = "BDT",
            CurrencySymbol = "৳",
            InvoiceFooter = "ধন্যবাদ, আবার আসবেন।"
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task<bool> SeedChartOfAccountsAsync(TahabilDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Accounts.AnyAsync(cancellationToken).ConfigureAwait(false)) return false;
        foreach (var account in SystemReferenceData.ChartOfAccounts)
        {
            context.Accounts.Add(new Account
            {
                Code = account.Code,
                NameBn = account.NameBn,
                NameEn = account.NameEn,
                AccountType = account.Type,
                IsSystem = true
            });
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task<bool> SeedPaymentMethodsAsync(TahabilDbContext context, CancellationToken cancellationToken)
    {
        if (await context.PaymentMethods.AnyAsync(cancellationToken).ConfigureAwait(false)) return false;
        foreach (var method in SystemReferenceData.PaymentMethods)
        {
            context.PaymentMethods.Add(new PaymentMethod
            {
                NameBn = method.NameBn,
                NameEn = method.NameEn,
                IsCash = method.IsCash,
                IsSystem = true,
                CashAccountCode = method.CashAccountCode,
                SortOrder = method.SortOrder
            });
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task<bool> SeedExpenseCategoriesAsync(TahabilDbContext context, CancellationToken cancellationToken)
    {
        if (await context.ExpenseCategories.AnyAsync(cancellationToken).ConfigureAwait(false)) return false;
        foreach (var category in SystemReferenceData.ExpenseCategories)
        {
            context.ExpenseCategories.Add(new ExpenseCategory
            {
                NameBn = category.NameBn,
                NameEn = category.NameEn,
                AccountCode = category.AccountCode,
                IsSystem = true,
                SortOrder = category.SortOrder
            });
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task<bool> SeedUnitsAsync(TahabilDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Units.AnyAsync(cancellationToken).ConfigureAwait(false)) return false;

        var piece = new Unit { NameBn = "পিস", NameEn = "Piece", ShortBn = "পিস", ShortEn = "pcs", FactorToBase = 1m, IsSystemUnit = true };
        var dozen = new Unit { NameBn = "ডজন", NameEn = "Dozen", ShortBn = "ডজন", ShortEn = "dz", FactorToBase = 12m, BaseUnitId = piece.Id, BaseUnit = piece, IsSystemUnit = true };
        var box = new Unit { NameBn = "বক্স", NameEn = "Box", ShortBn = "বক্স", ShortEn = "box", FactorToBase = 1m, BaseUnitId = piece.Id, BaseUnit = piece, IsSystemUnit = true };
        var carton = new Unit { NameBn = "কার্টন", NameEn = "Carton", ShortBn = "কার্টন", ShortEn = "ctn", FactorToBase = 12m, BaseUnitId = box.Id, BaseUnit = box, IsSystemUnit = true };
        var packet = new Unit { NameBn = "প্যাকেট", NameEn = "Packet", ShortBn = "প্যাকেট", ShortEn = "pkt", FactorToBase = 1m, BaseUnitId = piece.Id, BaseUnit = piece, IsSystemUnit = true };
        var gram = new Unit { NameBn = "গ্রাম", NameEn = "Gram", ShortBn = "গ্রাম", ShortEn = "g", FactorToBase = 1m, IsSystemUnit = true };
        var kilogram = new Unit { NameBn = "কেজি", NameEn = "Kilogram", ShortBn = "কেজি", ShortEn = "kg", FactorToBase = 1000m, BaseUnitId = gram.Id, BaseUnit = gram, IsSystemUnit = true };
        var milliliter = new Unit { NameBn = "মিলিলিটার", NameEn = "Milliliter", ShortBn = "মিলি", ShortEn = "ml", FactorToBase = 1m, IsSystemUnit = true };
        var liter = new Unit { NameBn = "লিটার", NameEn = "Liter", ShortBn = "লিটার", ShortEn = "l", FactorToBase = 1000m, BaseUnitId = milliliter.Id, BaseUnit = milliliter, IsSystemUnit = true };
        var meter = new Unit { NameBn = "মিটার", NameEn = "Meter", ShortBn = "মিটার", ShortEn = "m", FactorToBase = 1m, IsSystemUnit = true };

        context.Units.AddRange(piece, dozen, box, carton, packet, gram, kilogram, milliliter, liter, meter);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
