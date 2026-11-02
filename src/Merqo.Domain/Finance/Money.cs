namespace Merqo.Domain.Finance;

/// <summary>
/// Exact monetary amount expressed in the currency minor unit (poisha for BDT).
/// Monetary values never use <see cref="double"/> or <see cref="float"/>.
/// </summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    /// <summary>Number of fraction digits kept for a monetary amount.</summary>
    public const int Scale = 2;

    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = MoneyMath.Round(amount);
    }

    public static Money Zero => new(0m);

    public static Money FromDecimal(decimal amount) => new(amount);

    public bool IsZero => Amount == 0m;
    public bool IsNegative => Amount < 0m;
    public bool IsPositive => Amount > 0m;

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);
    public static Money operator -(Money a) => new(-a.Amount);
    public static Money operator *(Money a, decimal factor) => new(a.Amount * factor);
    public static Money operator *(decimal factor, Money a) => new(a.Amount * factor);
    public static bool operator ==(Money a, Money b) => a.Amount == b.Amount;
    public static bool operator !=(Money a, Money b) => a.Amount != b.Amount;
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    public static Money Sum(IEnumerable<Money> values)
    {
        decimal total = 0m;
        foreach (var value in values) total += value.Amount;
        return new Money(total);
    }

    public Money ClampToZero() => IsNegative ? Zero : this;

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);
    public bool Equals(Money other) => Amount == other.Amount;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => Amount.GetHashCode();
    public override string ToString() => Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
