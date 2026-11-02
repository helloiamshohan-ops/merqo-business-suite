using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Timestamps are persisted as round-trippable UTC ISO-8601 strings so that SQLite ordering is
/// lexicographic-correct and no local-time ambiguity can enter the ledger.
/// </summary>
public sealed class UtcDateTimeToStringConverter : ValueConverter<DateTime, string>
{
    public const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

    public UtcDateTimeToStringConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v.ToString(Format, CultureInfo.InvariantCulture) : v.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture),
            v => System.DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AdjustToUniversal))
    {
    }
}

public sealed class NullableUtcDateTimeToStringConverter : ValueConverter<DateTime?, string?>
{
    public NullableUtcDateTimeToStringConverter()
        : base(
            v => v == null ? null : (v.Value.Kind == DateTimeKind.Utc ? v.Value.ToString(UtcDateTimeToStringConverter.Format, CultureInfo.InvariantCulture) : v.Value.ToUniversalTime().ToString(UtcDateTimeToStringConverter.Format, CultureInfo.InvariantCulture)),
            v => v == null ? null : System.DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AdjustToUniversal))
    {
    }
}
