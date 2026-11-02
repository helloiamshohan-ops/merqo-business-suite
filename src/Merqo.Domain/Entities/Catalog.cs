namespace Merqo.Domain.Entities;

public sealed class Category : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public sealed class Brand : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>
/// Unit of measure. A base unit has a null <see cref="BaseUnitId"/> and a factor of 1;
/// derived units point at their parent and declare how many parent units they contain.
/// </summary>
public sealed class Unit : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string ShortBn { get; set; } = string.Empty;
    public string? ShortEn { get; set; }
    public Guid? BaseUnitId { get; set; }
    public Unit? BaseUnit { get; set; }
    public decimal FactorToBase { get; set; } = 1m;
    public bool IsSystemUnit { get; set; }
}

public sealed class Product : AuditableEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Sku { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal TaxRatePercent { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
}

public sealed class ProductBarcode : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
