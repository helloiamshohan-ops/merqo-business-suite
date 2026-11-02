using Merqo.Domain.Entities;
using Merqo.Infrastructure;
using Merqo.Infrastructure.Persistence;

namespace Merqo.Application.Tests;

/// <summary>Creates an isolated, fully seeded database with one product, customer and supplier.</summary>
public sealed class BusinessFixture : IAsyncDisposable
{
    private BusinessFixture(AppPaths paths)
    {
        Paths = paths;
    }

    public AppPaths Paths { get; }
    public Guid ProductId { get; private set; }
    public Guid SecondProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid CashMethodId { get; private set; }
    public Guid PieceUnitId { get; private set; }

    public static async Task<BusinessFixture> CreateAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "tahabil-app-tests", Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(root);
        await new DatabaseInitializer(paths).InitializeAsync();

        var fixture = new BusinessFixture(paths);
        await using (var context = TahabilDbContextFactory.Create(paths.DatabasePath))
        {
            var piece = await context.Units.FirstAsync(u => u.NameEn == "Piece");
            fixture.PieceUnitId = piece.Id;

            var product = new Product
            {
                NameBn = "মিনিকেট চাল ৫ কেজি",
                Code = "RICE-5",
                UnitId = piece.Id,
                PurchasePrice = 60m,
                SellingPrice = 75m,
                MinimumStock = 5m
            };
            var second = new Product
            {
                NameBn = "সয়াবিন তেল ১ লিটার",
                Code = "OIL-1",
                UnitId = piece.Id,
                PurchasePrice = 150m,
                SellingPrice = 168m,
                MinimumStock = 3m
            };
            var customer = new Customer { Name = "করিম স্টোর", Phone = "01711111111" };
            var supplier = new Supplier { Name = "মেসার্স রহমান ট্রেডার্স", Phone = "01822222222" };

            context.Products.AddRange(product, second);
            context.Customers.Add(customer);
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            fixture.ProductId = product.Id;
            fixture.SecondProductId = second.Id;
            fixture.CustomerId = customer.Id;
            fixture.SupplierId = supplier.Id;
            fixture.CashMethodId = (await context.PaymentMethods.FirstAsync(m => m.IsCash)).Id;
        }

        return fixture;
    }

    public TahabilDbContext NewContext() => TahabilDbContextFactory.Create(Paths.DatabasePath);

    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        try
        {
            if (Directory.Exists(Paths.Root)) Directory.Delete(Paths.Root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
