using Merqo.Application.Common;
using Merqo.Domain.Entities;
using Merqo.Domain.Finance;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

/// <summary>
/// Party ledgers are append-only. Balances are always derived from the entries, never stored as a
/// mutable counter, so a displayed due can be reconciled against the transactions that produced it.
/// </summary>
public sealed class LedgerService
{
    private readonly TahabilDbContext _context;

    public LedgerService(TahabilDbContext context) => _context = context;

    public async Task<decimal> GetCustomerBalanceAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var rows = await _context.CustomerLedgerEntries.AsNoTracking()
            .Where(e => e.CustomerId == customerId)
            .Select(e => new { e.Debit, e.Credit })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var customer = await _context.Customers.AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.OpeningBalance })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var opening = customer?.OpeningBalance ?? 0m;
        return MoneyMath.Round(opening + rows.Sum(r => r.Debit) - rows.Sum(r => r.Credit));
    }

    public async Task<decimal> GetSupplierBalanceAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var rows = await _context.SupplierLedgerEntries.AsNoTracking()
            .Where(e => e.SupplierId == supplierId)
            .Select(e => new { e.Debit, e.Credit })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var supplier = await _context.Suppliers.AsNoTracking()
            .Where(s => s.Id == supplierId)
            .Select(s => new { s.OpeningBalance })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var opening = supplier?.OpeningBalance ?? 0m;
        return MoneyMath.Round(opening + rows.Sum(r => r.Credit) - rows.Sum(r => r.Debit));
    }

    public async Task<CustomerLedgerEntry> PostCustomerEntryAsync(
        Guid customerId,
        LedgerEntryType entryType,
        decimal debit,
        decimal credit,
        DateTime entryDateUtc,
        string? referenceType,
        Guid? referenceId,
        string? note,
        CurrentUser user,
        CancellationToken cancellationToken = default)
    {
        if (debit < 0m || credit < 0m)
            throw new Domain.Common.DomainRuleViolationException(Domain.Common.DomainErrorCodes.NegativePrice, "Ledger amounts cannot be negative.");

        var current = await GetCustomerBalanceAsync(customerId, cancellationToken).ConfigureAwait(false);
        var entry = new CustomerLedgerEntry
        {
            CustomerId = customerId,
            EntryDateUtc = entryDateUtc,
            EntryType = entryType,
            Debit = MoneyMath.Round(debit),
            Credit = MoneyMath.Round(credit),
            BalanceAfter = MoneyMath.Round(current + debit - credit),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Note = note,
            CreatedByUserId = user.UserId
        };
        _context.CustomerLedgerEntries.Add(entry);
        return entry;
    }

    public async Task<SupplierLedgerEntry> PostSupplierEntryAsync(
        Guid supplierId,
        LedgerEntryType entryType,
        decimal debit,
        decimal credit,
        DateTime entryDateUtc,
        string? referenceType,
        Guid? referenceId,
        string? note,
        CurrentUser user,
        CancellationToken cancellationToken = default)
    {
        if (debit < 0m || credit < 0m)
            throw new Domain.Common.DomainRuleViolationException(Domain.Common.DomainErrorCodes.NegativePrice, "Ledger amounts cannot be negative.");

        var current = await GetSupplierBalanceAsync(supplierId, cancellationToken).ConfigureAwait(false);
        var entry = new SupplierLedgerEntry
        {
            SupplierId = supplierId,
            EntryDateUtc = entryDateUtc,
            EntryType = entryType,
            Debit = MoneyMath.Round(debit),
            Credit = MoneyMath.Round(credit),
            BalanceAfter = MoneyMath.Round(current + credit - debit),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Note = note,
            CreatedByUserId = user.UserId
        };
        _context.SupplierLedgerEntries.Add(entry);
        return entry;
    }
}
