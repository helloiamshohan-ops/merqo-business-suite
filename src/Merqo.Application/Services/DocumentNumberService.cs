using System.Globalization;
using Merqo.Domain.Entities;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

/// <summary>
/// Allocates gap-free document numbers inside the caller's transaction. The unique index on
/// (sequence_key, year, month) is the real guarantee — the code never relies on a cached counter.
/// </summary>
public sealed class DocumentNumberService
{
    public const string SaleKey = "sale";
    public const string PurchaseKey = "purchase";
    public const string SaleReturnKey = "sale_return";
    public const string PurchaseReturnKey = "purchase_return";
    public const string ExpenseKey = "expense";
    public const string AdjustmentKey = "stock_adjustment";
    public const string JournalKey = "journal";

    private readonly TahabilDbContext _context;

    public DocumentNumberService(TahabilDbContext context) => _context = context;

    public async Task<string> NextAsync(string sequenceKey, string format, DateTime localDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sequenceKey)) throw new ArgumentException("Sequence key is required.", nameof(sequenceKey));

        var year = localDate.Year;
        var month = localDate.Month;

        var sequence = await _context.DocumentSequences
            .FirstOrDefaultAsync(s => s.SequenceKey == sequenceKey && s.Year == year && s.Month == month, cancellationToken)
            .ConfigureAwait(false);

        if (sequence is null)
        {
            sequence = new DocumentSequence { SequenceKey = sequenceKey, Year = year, Month = month, LastNumber = 0 };
            _context.DocumentSequences.Add(sequence);
        }

        sequence.LastNumber += 1;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Format(format, localDate, sequence.LastNumber);
    }

    public static string Format(string format, DateTime date, long number)
    {
        if (string.IsNullOrWhiteSpace(format)) format = "{SEQ:5}";

        var result = format
            .Replace("{YYYY}", date.Year.ToString("0000", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{YY}", (date.Year % 100).ToString("00", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{MM}", date.Month.ToString("00", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{DD}", date.Day.ToString("00", CultureInfo.InvariantCulture), StringComparison.Ordinal);

        // {SEQ:n} pads the counter to n digits.
        var start = result.IndexOf("{SEQ", StringComparison.Ordinal);
        while (start >= 0)
        {
            var end = result.IndexOf('}', start);
            if (end < 0) break;

            var token = result.Substring(start, end - start + 1);
            var digits = 4;
            var colon = token.IndexOf(':');
            if (colon > 0)
            {
                var inner = token.Substring(colon + 1).TrimEnd('}');
                if (int.TryParse(inner, out var parsed) && parsed > 0) digits = parsed;
            }

            result = result.Replace(token, number.ToString("D" + digits, CultureInfo.InvariantCulture), StringComparison.Ordinal);
            start = result.IndexOf("{SEQ", StringComparison.Ordinal);
        }

        return result;
    }
}
