using FluentAssertions;
using Merqo.Domain.Catalog;
using Merqo.Domain.Common;
using Xunit;

namespace Merqo.Domain.Tests;

public sealed class UnitConversionTests
{
    private static readonly Guid Piece = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Dozen = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Carton = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static UnitConversionResolver StandardResolver() => new(new[]
    {
        new UnitDefinition(Piece, "পিস", "Piece", "পিস", "pcs", null, 1m),
        new UnitDefinition(Dozen, "ডজন", "Dozen", "ডজন", "dz", Piece, 12m),
        new UnitDefinition(Carton, "কার্টন", "Carton", "কার্টন", "ctn", Dozen, 10m)
    });

    [Fact]
    public void Dozen_resolves_to_twelve_pieces()
        => StandardResolver().FactorToBase(Dozen).Should().Be(12m);

    [Fact]
    public void Carton_resolves_through_dozen_to_base()
        => StandardResolver().FactorToBase(Carton).Should().Be(120m);

    [Fact]
    public void Conversion_between_units_is_correct()
        => StandardResolver().Convert(2m, Carton, Piece).Should().Be(240m);

    [Fact]
    public void Circular_chains_are_rejected()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var resolver = new UnitConversionResolver(new[]
        {
            new UnitDefinition(a, "ক", "A", "ক", "a", b, 2m),
            new UnitDefinition(b, "খ", "B", "খ", "b", a, 2m)
        });

        var act = () => resolver.FactorToBase(a);
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.CircularUnitConversion);
    }

    [Fact]
    public void Missing_parent_unit_is_rejected()
    {
        var orphan = Guid.NewGuid();
        var resolver = new UnitConversionResolver(new[]
        {
            new UnitDefinition(orphan, "অ", "Orphan", "অ", "o", Guid.NewGuid(), 3m)
        });

        var act = () => resolver.FactorToBase(orphan);
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.UnitConversionNotFound);
    }

    [Fact]
    public void Non_positive_conversion_factor_is_rejected()
    {
        var act = () => new UnitConversionResolver(new[] { new UnitDefinition(Guid.NewGuid(), "শূন্য", "Zero", "শূ", "z", null, 0m) });
        act.Should().Throw<DomainRuleViolationException>().Which.Code.Should().Be(DomainErrorCodes.InvalidConversionFactor);
    }
}
