namespace Merqo.Application.Common;

public sealed record CurrentUser(Guid? UserId, string Username, IReadOnlySet<string> Permissions, bool IsOwner)
{
    public static CurrentUser System { get; } = new(null, "system", new HashSet<string>(StringComparer.OrdinalIgnoreCase), true);

    public bool Has(string permission) => IsOwner || Permissions.Contains(permission);
}

public static class Permissions
{
    public const string DashboardView = "dashboard.view";
    public const string SaleCreate = "sales.create";
    public const string SaleView = "sales.view";
    public const string SaleVoid = "sales.void";
    public const string SaleReturnCreate = "sales.return";
    public const string PurchaseCreate = "purchases.create";
    public const string PaymentRecord = "payments.record";
    public const string ProductManage = "products.manage";
    public const string ProductPriceEdit = "products.price.edit";
    public const string StockAdjust = "stock.adjust";
    public const string StockNegativeOverride = "stock.negative.override";
    public const string CustomerManage = "customers.manage";
    public const string SupplierManage = "suppliers.manage";
    public const string ExpenseManage = "expenses.manage";
    public const string ReportView = "reports.view";
    public const string ReportExport = "reports.export";
    public const string DatabaseBackup = "database.backup";
    public const string DatabaseRestore = "database.restore";
    public const string UserManage = "users.manage";
    public const string SettingsManage = "settings.manage";
    public const string AuditView = "audit.view";
    public const string LicenseView = "license.view";

    public static IReadOnlyList<string> All { get; } = new[]
    {
        DashboardView, SaleCreate, SaleView, SaleVoid, SaleReturnCreate, PurchaseCreate, PaymentRecord,
        ProductManage, ProductPriceEdit, StockAdjust, StockNegativeOverride, CustomerManage, SupplierManage,
        ExpenseManage, ReportView, ReportExport, DatabaseBackup, DatabaseRestore, UserManage, SettingsManage,
        AuditView, LicenseView
    };
}
