namespace Merqo.Domain.Finance;

/// <summary>
/// Documented rounding and allocation rules for all financial arithmetic.
/// </summary>
/// <remarks>
/// Rounding policy (see docs/ACCOUNTING_MODEL.md):
/// every individual monetary figure is rounded half-away-from-zero to 2 decimals;
/// intermediate multiplications are performed in decimal and rounded only once, at the point
/// the figure becomes a stored/posted amount. Allocations always sum exactly to the allocated
/// total so that no poisha is created or lost during pro-rata distribution.
/// </remarks>
public static class MoneyMath
{
    public const int Scale = Money.Scale;

    public static decimal Round(decimal value, int scale = Scale, MidpointRounding mode = MidpointRounding.AwayFromZero)
        => decimal.Round(value, scale, mode);

    public static decimal RoundToTaka(decimal value) => Round(value, 0);

    /// <summary>
    /// Distributes <paramref name="total"/> across <paramref name="weights"/> so that the returned
    /// parts always sum exactly to the (already rounded) total. The rounding remainder is applied to the
    /// last weight that is not zero.
    /// </summary>
    public static decimal[] Allocate(decimal total, IReadOnlyList<decimal> weights, int scale = Scale)
    {
        ArgumentNullException.ThrowIfNull(weights);
        var result = new decimal[weights.Count];
        if (weights.Count == 0) return result;

        decimal sumWeights = 0m;
        foreach (var weight in weights) sumWeights += weight;

        if (sumWeights == 0m)
        {
            result[0] = Round(total, scale);
            return result;
        }

        decimal allocated = 0m;
        int remainingIndex = -1;
        for (int i = 0; i < weights.Count; i++)
        {
            var share = Round(total * weights[i] / sumWeights, scale);
            result[i] = share;
            allocated += share;
            if (weights[i] != 0m) remainingIndex = i;
        }

        var remainder = Round(total, scale) - allocated;
        if (remainder != 0m && remainingIndex >= 0) result[remainingIndex] += remainder;
        return result;
    }
}
