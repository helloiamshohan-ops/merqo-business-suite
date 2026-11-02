using Merqo.Domain.Entities;
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

    public DatabaseInitializer(AppPaths paths) => _paths = paths;

    public async Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        _paths.EnsureCreated();
        var existedBefore = File.Exists(_paths.DatabasePath);

        await using var context = TahabilDbContextFactory.Create(_paths.DatabasePath);

        // Once EF migrations are committed the schema is upgraded in place; before the first migration
        // exists the schema is created directly so that development builds stay runnable.
        if (context.Database.GetMigrations().Any())
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        }

        var seededUnits = await SeedUnitsAsync(context, cancellationToken).ConfigureAwait(false);
        await SeedBusinessSettingsAsync(context, cancellationToken).ConfigureAwait(false);

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
