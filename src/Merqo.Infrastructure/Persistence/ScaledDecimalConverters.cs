using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Monetary amounts, quantities, rates and conversion factors are stored as scaled 64-bit integers
/// (4 decimal places) instead of SQLite REAL/TEXT. This keeps every value exact, makes SQL SUM/ORDER BY
/// correct, and avoids binary floating point entirely.
/// </summary>
public static class DecimalScale
{
    public const int Places = 4;
    public const long Factor = 10_000L;

    public static long ToScaled(decimal value)
        => (long)decimal.Round(value * Factor, 0, MidpointRounding.AwayFromZero);

    public static decimal FromScaled(long value)
        => value / (decimal)Factor;
}

public sealed class DecimalToScaledLongConverter : ValueConverter<decimal, long>
{
    public DecimalToScaledLongConverter()
        : base(v => DecimalScale.ToScaled(v), v => DecimalScale.FromScaled(v))
    {
    }
}

public sealed class NullableDecimalToScaledLongConverter : ValueConverter<decimal?, long?>
{
    public NullableDecimalToScaledLongConverter()
        : base(v => v == null ? null : DecimalScale.ToScaled(v.Value), v => v == null ? null : DecimalScale.FromScaled(v.Value))
    {
    }
}
