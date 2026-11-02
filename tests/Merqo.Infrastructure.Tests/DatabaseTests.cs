using FluentAssertions;
using Merqo.Domain.Entities;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Merqo.Infrastructure.Tests;

public sealed class DatabaseTests
{
    [Fact]
    public async Task Initialization_creates_schema_and_seeds_system_units()
    {
        await using var db = await DatabaseTestContext.CreateAsync();

        db.Result.Created.Should().BeTrue();
        db.Result.IntegrityOk.Should().BeTrue();
        db.Result.SeededUnits.Should().BeTrue();
        File.Exists(db.Paths.DatabasePath).Should().BeTrue();

        var units = await db.Context.Units.CountAsync();
        units.Should().Be(10);

        var settings = await db.Context.BusinessSettings.SingleAsync();
        settings.CurrencyCode.Should().Be("BDT");
        settings.CurrencySymbol.Should().Be("৳");
    }

    [Fact]
    public async Task Reinitialization_is_idempotent()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        var second = await new DatabaseInitializer(db.Paths).InitializeAsync();

        second.SeededUnits.Should().BeFalse();
        second.IntegrityOk.Should().BeTrue();
        (await db.Context.Units.CountAsync()).Should().Be(10);
        (await db.Context.BusinessSettings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Wal_mode_and_foreign_keys_are_active()
    {
        await using var db = await DatabaseTestContext.CreateAsync();

        (await SqliteDiagnostics.JournalModeAsync(db.Context)).Should().Be("wal");
        (await SqliteDiagnostics.ForeignKeysAsync(db.Context)).Should().Be(1);
        (await SqliteDiagnostics.SynchronousAsync(db.Context)).Should().Be(2); // FULL
    }

    [Fact]
    public async Task Decimal_values_roundtrip_exactly_and_are_stored_as_integers()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        var unit = await db.Context.Units.FirstAsync(u => u.NameEn == "Piece");

        var product = new Product
        {
            NameBn = "মিনিকেট চাল ৫ কেজি",
            Code = "RICE-5KG",
            UnitId = unit.Id,
            PurchasePrice = 1234.56m,
            SellingPrice = 0.0001m,
            WholesalePrice = 999999.99m,
            MinimumStock = 2.5m
        };
        db.Context.Products.Add(product);
        await db.Context.SaveChangesAsync();

        await using var verify = db.NewContext();
        var loaded = await verify.Products.SingleAsync(p => p.Code == "RICE-5KG");
        loaded.PurchasePrice.Should().Be(1234.56m);
        loaded.SellingPrice.Should().Be(0.0001m);
        loaded.WholesalePrice.Should().Be(999999.99m);
        loaded.MinimumStock.Should().Be(2.5m);

        var storageType = await SqliteDiagnostics.StorageClassAsync(verify, "products", "purchase_price", "code = 'RICE-5KG'");
        storageType.Should().Be("integer");
    }

    [Fact]
    public async Task Duplicate_product_code_is_rejected_by_unique_index()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        var unit = await db.Context.Units.FirstAsync(u => u.NameEn == "Piece");

        db.Context.Products.Add(new Product { NameBn = "ক", Code = "DUP", UnitId = unit.Id });
        await db.Context.SaveChangesAsync();

        db.Context.Products.Add(new Product { NameBn = "খ", Code = "DUP", UnitId = unit.Id });
        var act = async () => await db.Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Foreign_key_violation_is_rejected()
    {
        await using var db = await DatabaseTestContext.CreateAsync();

        db.Context.ProductBarcodes.Add(new ProductBarcode { ProductId = Guid.NewGuid(), Barcode = "8801234567890" });
        var act = async () => await db.Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Archived_products_are_hidden_from_default_queries_but_retained()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        var unit = await db.Context.Units.FirstAsync(u => u.NameEn == "Piece");

        var product = new Product { NameBn = "আর্কাইভ পণ্য", Code = "ARCH-1", UnitId = unit.Id };
        db.Context.Products.Add(product);
        await db.Context.SaveChangesAsync();

        product.IsArchived = true;
        product.ArchivedAtUtc = DateTime.UtcNow;
        await db.Context.SaveChangesAsync();

        (await db.Context.Products.CountAsync()).Should().Be(0);
        (await db.Context.Products.IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Failed_transaction_leaves_no_partial_rows()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        var unit = await db.Context.Units.FirstAsync(u => u.NameEn == "Piece");

        await using (var transaction = await db.Context.Database.BeginTransactionAsync())
        {
            db.Context.Products.Add(new Product { NameBn = "রোলব্যাক পণ্য", Code = "RB-1", UnitId = unit.Id });
            await db.Context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        await using var verify = db.NewContext();
        (await verify.Products.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Integrity_check_reports_ok_on_healthy_database()
    {
        await using var db = await DatabaseTestContext.CreateAsync();
        (await DatabaseInitializer.CheckIntegrityAsync(db.Context)).Should().BeTrue();
    }
}
