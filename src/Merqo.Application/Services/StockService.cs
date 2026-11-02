using Merqo.Application.Common;
using Merqo.Domain.Entities;
using Merqo.Domain.Finance;
using Merqo.Domain.Inventory;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

public sealed record StockIssue(decimal BaseQuantity, decimal FrozenUnitCost, decimal CostAmount, decimal BalanceAfter);
public sealed record StockReceipt(decimal BaseQuantity, decimal UnitCost, decimal NewAverageCost, decimal BalanceAfter);

/// <summary>
/// The only writer of <see cref="StockBalance"/> and <see cref="StockMovement"/>. Every quantity change
/// is paired with an immutable movement row carrying its source document, so stock can never drift
/// silently and COGS is reproducible from history.
/// </summary>
public sealed class StockService
{
    private readonly TahabilDbContext _context;

    public StockService(TahabilDbContext context) => _context = context;

    public async Task<StockBalance> GetOrCreateBalanceAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var balance = await _context.StockBalances
            .FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken)
            .ConfigureAwait(false);

        if (balance is not null) return balance;

        balance = new StockBalance { ProductId = productId, QuantityOnHand = 0m, AverageCost = 0m };
        _context.StockBalances.Add(balance);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return balance;
    }

    /// <summary>Issues stock out of the shop (sale, purchase return, decrease adjustment) at the frozen average cost.</summary>
    public async Task<StockIssue> IssueAsync(
        Guid productId,
        decimal baseQuantity,
        StockMovementType movementType,
        string? referenceType,
        Guid? referenceId,
        string? referenceNumber,
        bool allowNegativeStock,
        CurrentUser user,
        string productLabel,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetOrCreateBalanceAsync(productId, cancellationToken).ConfigureAwait(false);
        StockPolicy.EnsureCanIssue(balance.QuantityOnHand, baseQuantity, allowNegativeStock, productLabel);

        var frozenUnitCost = MoneyMath.Round(balance.AverageCost);
        var costAmount = MoneyMath.Round(baseQuantity * balance.AverageCost);
        balance.QuantityOnHand = decimal.Round(balance.QuantityOnHand - baseQuantity, 4, MidpointRounding.AwayFromZero);
        balance.UpdatedAtUtc = DateTime.UtcNow;
        balance.Version++;

        _context.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            MovementType = movementType,
            QuantityOut = baseQuantity,
            UnitCost = frozenUnitCost,
            BalanceAfter = balance.QuantityOnHand,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNumber = referenceNumber,
            CreatedByUserId = user.UserId,
            MovedAtUtc = DateTime.UtcNow
        });

        return new StockIssue(baseQuantity, frozenUnitCost, costAmount, balance.QuantityOnHand);
    }

    /// <summary>Receives stock into the shop (purchase, sale return, increase adjustment) and re-averages the cost.</summary>
    public async Task<StockReceipt> ReceiveAsync(
        Guid productId,
        decimal baseQuantity,
        decimal unitCost,
        StockMovementType movementType,
        string? referenceType,
        Guid? referenceId,
        string? referenceNumber,
        CurrentUser user,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetOrCreateBalanceAsync(productId, cancellationToken).ConfigureAwait(false);

        var newAverage = WeightedAverageCost.NewAverage(balance.QuantityOnHand, balance.AverageCost, baseQuantity, unitCost);
        balance.QuantityOnHand = decimal.Round(balance.QuantityOnHand + baseQuantity, 4, MidpointRounding.AwayFromZero);
        balance.AverageCost = decimal.Round(newAverage, 4, MidpointRounding.AwayFromZero);
        balance.UpdatedAtUtc = DateTime.UtcNow;
        balance.Version++;

        _context.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            MovementType = movementType,
            QuantityIn = baseQuantity,
            UnitCost = MoneyMath.Round(unitCost),
            BalanceAfter = balance.QuantityOnHand,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ReferenceNumber = referenceNumber,
            CreatedByUserId = user.UserId,
            MovedAtUtc = DateTime.UtcNow
        });

        return new StockReceipt(baseQuantity, MoneyMath.Round(unitCost), balance.AverageCost, balance.QuantityOnHand);
    }
}
