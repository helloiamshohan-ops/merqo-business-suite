using FluentAssertions;
using Merqo.Domain.Finance;
using Xunit;

namespace Merqo.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Rounds_half_away_from_zero_to_two_decimals()
    {
        new Money(10.005m).Amount.Should().Be(10.01m);
        new Money(10.004m).Amount.Should().Be(10.00m);
        new Money(-10.005m).Amount.Should().Be(-10.01m);
    }

    [Fact]
    public void Addition_and_multiplication_are_exact()
    {
        var a = new Money(0.10m);
        var b = new Money(0.20m);
        (a + b).Amount.Should().Be(0.30m);
        (a * 3m).Amount.Should().Be(0.30m);
    }

    [Fact]
    public void Sum_uses_decimal_accumulation()
    {
        var values = Enumerable.Range(0, 10).Select(_ => new Money(0.10m));
        Money.Sum(values).Amount.Should().Be(1.00m);
    }

    [Fact]
    public void Allocate_distributes_remainder_and_preserves_total()
    {
        var parts = MoneyMath.Allocate(100m, new[] { 1m, 1m, 1m });
        parts.Sum().Should().Be(100m);
        parts.Should().Equal(33.33m, 33.33m, 33.34m);
    }

    [Fact]
    public void Allocate_handles_zero_weights_without_losing_money()
    {
        var parts = MoneyMath.Allocate(50m, new[] { 0m, 0m });
        parts.Sum().Should().Be(50m);
    }

    [Theory]
    [InlineData(1234.565, 1234.57)]
    [InlineData(0.005, 0.01)]
    [InlineData(0.004, 0.00)]
    public void Round_follows_documented_policy(decimal input, decimal expected)
        => MoneyMath.Round(input).Should().Be(expected);
}
