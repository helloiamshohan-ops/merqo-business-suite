using Merqo.Domain.Entities;

namespace Merqo.Domain.Accounting;

public sealed record AccountSeed(string Code, string NameBn, string NameEn, AccountType Type);

public sealed record PaymentMethodSeed(string NameBn, string NameEn, bool IsCash, string CashAccountCode, int SortOrder);

public sealed record ExpenseCategorySeed(string NameBn, string NameEn, string AccountCode, int SortOrder);

/// <summary>
/// System reference data that every installation needs on day one. This is configuration, not sample
/// business data: no customers, products or transactions are ever seeded.
/// </summary>
public static class SystemReferenceData
{
    public static IReadOnlyList<AccountSeed> ChartOfAccounts { get; } = new[]
    {
        new AccountSeed("1000", "নগদ", "Cash in hand", AccountType.Asset),
        new AccountSeed("1010", "ব্যাংক / মোবাইল ব্যাংকিং", "Bank / mobile banking", AccountType.Asset),
        new AccountSeed("1100", "ক্রেতা থেকে পাওনা", "Accounts receivable", AccountType.Asset),
        new AccountSeed("1200", "মজুত পণ্য", "Inventory", AccountType.Asset),
        new AccountSeed("1500", "দোকানের সরঞ্জাম", "Shop equipment", AccountType.Asset),
        new AccountSeed("2000", "সরবরাহকারীকে দেয়", "Accounts payable", AccountType.Liability),
        new AccountSeed("2100", "প্রদেয় ভ্যাট", "VAT payable", AccountType.Liability),
        new AccountSeed("2200", "ক্রেতার অগ্রিম", "Customer advance", AccountType.Liability),
        new AccountSeed("3000", "মালিকের মূলধন", "Owner's capital", AccountType.Equity),
        new AccountSeed("3100", "সঞ্চিত আয়", "Retained earnings", AccountType.Equity),
        new AccountSeed("4000", "বিক্রয় আয়", "Sales revenue", AccountType.Revenue),
        new AccountSeed("4010", "বিক্রয় ফেরত", "Sales returns", AccountType.Revenue),
        new AccountSeed("4020", "বিক্রয় ছাড়", "Sales discounts", AccountType.Revenue),
        new AccountSeed("4030", "রাউন্ডিং পার্থক্য", "Rounding difference", AccountType.Revenue),
        new AccountSeed("5000", "বিক্রিত পণ্যের ব্যয়", "Cost of goods sold", AccountType.Expense),
        new AccountSeed("6000", "দোকান ভাড়া", "Shop rent", AccountType.Expense),
        new AccountSeed("6010", "বিদ্যুৎ ও পানি", "Utilities", AccountType.Expense),
        new AccountSeed("6020", "কর্মচারী বেতন", "Employee salary", AccountType.Expense),
        new AccountSeed("6030", "পরিবহন", "Transport", AccountType.Expense),
        new AccountSeed("6040", "মেরামত ও রক্ষণাবেক্ষণ", "Repair and maintenance", AccountType.Expense),
        new AccountSeed("6090", "বিবিধ খরচ", "Miscellaneous expense", AccountType.Expense),
        new AccountSeed("6100", "মজুত ঘাটতি", "Inventory shrinkage", AccountType.Expense)
    };

    public static IReadOnlyList<PaymentMethodSeed> PaymentMethods { get; } = new[]
    {
        new PaymentMethodSeed("নগদ", "Cash", true, "1000", 1),
        new PaymentMethodSeed("বিকাশ", "bKash", false, "1010", 2),
        new PaymentMethodSeed("নগদ (মোবাইল)", "Nagad", false, "1010", 3),
        new PaymentMethodSeed("ব্যাংক ট্রান্সফার", "Bank transfer", false, "1010", 4),
        new PaymentMethodSeed("কার্ড", "Card", false, "1010", 5)
    };

    public static IReadOnlyList<ExpenseCategorySeed> ExpenseCategories { get; } = new[]
    {
        new ExpenseCategorySeed("দোকান ভাড়া", "Shop rent", "6000", 1),
        new ExpenseCategorySeed("বিদ্যুৎ ও পানি", "Utilities", "6010", 2),
        new ExpenseCategorySeed("কর্মচারী বেতন", "Employee salary", "6020", 3),
        new ExpenseCategorySeed("পরিবহন", "Transport", "6030", 4),
        new ExpenseCategorySeed("মেরামত ও রক্ষণাবেক্ষণ", "Repair and maintenance", "6040", 5),
        new ExpenseCategorySeed("বিবিধ খরচ", "Miscellaneous", "6090", 6)
    };
}
