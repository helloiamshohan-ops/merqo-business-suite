using Merqo.Domain.Common;

namespace Merqo.Domain.Catalog;

/// <summary>
/// A unit of measure. <see cref="FactorToBase"/> expresses how many base units one of this unit contains.
/// Base units have a null <see cref="BaseUnitId"/> and a factor of exactly 1.
/// </summary>
public sealed record UnitDefinition(Guid Id, string NameBn, string NameEn, string ShortBn, string ShortEn, Guid? BaseUnitId, decimal FactorToBase)
{
    public bool IsBase => BaseUnitId is null;
}

/// <summary>
/// Resolves unit hierarchies to a single base unit. Rejects circular chains, missing links and
/// non-positive factors so that "one dozen" can never silently reduce the wrong number of pieces.
/// </summary>
public sealed class UnitConversionResolver
{
    public const int MaxDepth = 8;

    private readonly Dictionary<Guid, UnitDefinition> _units;

    public UnitConversionResolver(IEnumerable<UnitDefinition> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        _units = new Dictionary<Guid, UnitDefinition>();
        foreach (var unit in units)
        {
            if (unit.FactorToBase <= 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.InvalidConversionFactor, $"Unit '{unit.NameEn}' must have a positive conversion factor.");
            _units[unit.Id] = unit;
        }
    }

    /// <summary>Factor that converts a quantity of <paramref name="unitId"/> into the base unit quantity.</summary>
    public decimal FactorToBase(Guid unitId)
    {
        if (!_units.TryGetValue(unitId, out var unit))
            throw new DomainRuleViolationException(DomainErrorCodes.UnitConversionNotFound, $"Unit '{unitId}' is not defined.");

        decimal factor = 1m;
        var visited = new HashSet<Guid>();
        var current = unit;
        int depth = 0;

        while (true)
        {
            if (!visited.Add(current.Id))
                throw new DomainRuleViolationException(DomainErrorCodes.CircularUnitConversion, $"Circular unit conversion detected at '{current.NameEn}'.");
            if (++depth > MaxDepth)
                throw new DomainRuleViolationException(DomainErrorCodes.CircularUnitConversion, $"Unit conversion chain is too deep for '{unit.NameEn}'.");

            factor *= current.FactorToBase;
            if (decimal.Round(factor, 4, MidpointRounding.AwayFromZero) != factor) factor = decimal.Round(factor, 4, MidpointRounding.AwayFromZero);

            if (current.IsBase) return factor;

            if (!_units.TryGetValue(current.BaseUnitId!.Value, out var parent))
                throw new DomainRuleViolationException(DomainErrorCodes.UnitConversionNotFound, $"Base unit of '{current.NameEn}' is not defined.");
            current = parent;
        }
    }

    /// <summary>Converts a quantity between two units of the same hierarchy.</summary>
    public decimal Convert(decimal quantity, Guid fromUnitId, Guid toUnitId)
    {
        var fromFactor = FactorToBase(fromUnitId);
        var toFactor = FactorToBase(toUnitId);
        return decimal.Round(quantity * fromFactor / toFactor, 4, MidpointRounding.AwayFromZero);
    }
}
