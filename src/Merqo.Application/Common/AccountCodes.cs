
namespace Merqo.Application.Common;

/// <summary>Chart-of-accounts codes referenced by the posting rules. See docs/ACCOUNTING_MODEL.md.</summary>
public static class AccountCodes
{
    public const string Cash = "1000";
    public const string Bank = "1010";
    public const string AccountsReceivable = "1100";
    public const string Inventory = "1200";
    public const string AccountsPayable = "2000";
    public const string VatPayable = "2100";
    public const string SalesRevenue = "4000";
    public const string SalesReturns = "4010";
    public const string SalesDiscounts = "4020";
    public const string RoundingDifference = "4030";
    public const string CostOfGoodsSold = "5000";
    public const string InventoryShrinkage = "6100";
}
