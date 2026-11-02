using Merqo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Infrastructure.Persistence;

public sealed class TahabilDbContext : DbContext
{
    public const string DatabaseFileName = "tahabil.db";

    public TahabilDbContext(DbContextOptions<TahabilDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<BusinessSettings> BusinessSettings => Set<BusinessSettings>();
    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HaveConversion<DecimalToScaledLongConverter>();
        configurationBuilder.Properties<decimal?>().HaveConversion<NullableDecimalToScaledLongConverter>();
        configurationBuilder.Properties<string>().HaveMaxLength(500);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeToStringConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeToStringConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(200);
            e.Property(x => x.NameEn).HasMaxLength(200);
            e.HasIndex(x => x.NameBn).IsUnique();
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<Brand>(e =>
        {
            e.ToTable("brands");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.NameBn).IsUnique();
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<Unit>(e =>
        {
            e.ToTable("units");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(100);
            e.Property(x => x.ShortBn).IsRequired().HasMaxLength(30);
            e.HasIndex(x => x.NameBn).IsUnique();
            e.HasOne(x => x.BaseUnit).WithMany().HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(300);
            e.Property(x => x.Code).IsRequired().HasMaxLength(60);
            e.Property(x => x.Sku).HasMaxLength(60);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.NameBn);
            e.HasIndex(x => new { x.IsActive, x.IsArchived });
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand).WithMany(b => b.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<ProductBarcode>(e =>
        {
            e.ToTable("product_barcodes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Barcode).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.Barcode).IsUnique();
            e.HasOne(x => x.Product).WithMany(p => p.Barcodes).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BusinessSettings>(e =>
        {
            e.ToTable("business_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.BusinessName).IsRequired().HasMaxLength(300);
            e.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(10);
        });

        modelBuilder.Entity<ApplicationSetting>(e =>
        {
            e.ToTable("application_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).IsRequired().HasMaxLength(150);
            e.Property(x => x.Value).IsRequired().HasMaxLength(2000);
            e.HasIndex(x => x.Key).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
        SnakeCaseNaming.Apply(modelBuilder);
    }
}
