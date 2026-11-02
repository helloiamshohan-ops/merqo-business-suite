using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Timestamps are persisted as round-trippable UTC ISO-8601 strings so that SQLite ordering is
/// lexicographic-correct and no local-time ambiguity can ever enter the ledger.
/// </summary>
public sealed class UtcDateTimeToStringConverter : ValueConverter<DateTime, string>
{
    public const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

    public UtcDateTimeToStringConverter()
        : base(v => Format(v), v => Parse(v))
    {
    }

    public static string Format(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
        return utc.ToString(Format, CultureInfo.InvariantCulture);
    }

    public static DateTime Parse(string value)
    {
        var parsed = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return parsed.Kind == DateTimeKind.Utc ? parsed : DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }
}

public sealed class NullableUtcDateTimeToStringConverter : ValueConverter<DateTime?, string?>
{
    public NullableUtcDateTimeToStringConverter()
        : base(
            v => v == null ? null : UtcDateTimeToStringConverter.Format(v.Value),
            v => v == null ? null : UtcDateTimeToStringConverter.Parse(v))
    {
    }
}
