namespace Merqo.Domain.Common;

/// <summary>
/// Raised when a domain invariant or business rule is violated.
/// The <see cref="Code"/> is stable and is mapped to a natural Bangla message by the presentation layer,
/// so domain code never has to carry user-facing wording.
/// </summary>
public sealed class DomainRuleViolationException : Exception
{
    public string Code { get; }

    public DomainRuleViolationException(string code, string developerMessage)
        : base(developerMessage)
    {
        Code = code;
    }
}

/// <summary>Stable rule codes shared by domain, application and tests.</summary>
public static class DomainErrorCodes
{
    public const string QuantityNotPositive = "DOMAIN.QUANTITY_NOT_POSITIVE";
    public const string NegativePrice = "DOMAIN.NEGATIVE_PRICE";
    public const string NegativeDiscount = "DOMAIN.NEGATIVE_DISCOUNT";
    public const string DiscountExceedsAmount = "DOMAIN.DISCOUNT_EXCEEDS_AMOUNT";
    public const string InvalidTaxRate = "DOMAIN.INVALID_TAX_RATE";
    public const string EmptySale = "DOMAIN.EMPTY_SALE";
    public const string NegativePayment = "DOMAIN.NEGATIVE_PAYMENT";
    public const string InsufficientStock = "DOMAIN.INSUFFICIENT_STOCK";
    public const string CircularUnitConversion = "DOMAIN.CIRCULAR_UNIT_CONVERSION";
    public const string InvalidConversionFactor = "DOMAIN.INVALID_CONVERSION_FACTOR";
    public const string UnitConversionNotFound = "DOMAIN.UNIT_CONVERSION_NOT_FOUND";
    public const string JournalNotBalanced = "DOMAIN.JOURNAL_NOT_BALANCED";
    public const string JournalTooFewLines = "DOMAIN.JOURNAL_TOO_FEW_LINES";
    public const string NegativeQuantity = "DOMAIN.NEGATIVE_QUANTITY";
}
