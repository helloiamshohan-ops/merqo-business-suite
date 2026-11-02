using Merqo.Application.Common;
using Merqo.Domain.Accounting;
using Merqo.Domain.Common;
using Merqo.Domain.Entities;
using Merqo.Domain.Finance;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Application.Services;

public sealed record JournalLineDraft(string AccountCode, decimal Debit, decimal Credit, string? Memo = null, Guid? CustomerId = null, Guid? SupplierId = null);

/// <summary>
/// Owns the chart of accounts and posts balanced journal entries. Nothing else in the product writes
/// to the journal directly, and nothing may post an unbalanced entry.
/// </summary>
public sealed class AccountingService
{
    private readonly TahabilDbContext _context;

    public AccountingService(TahabilDbContext context) => _context = context;

    public async Task EnsureChartOfAccountsAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _context.Accounts.Select(a => a.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
        var known = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        foreach (var account in SystemReferenceData.ChartOfAccounts)
        {
            if (known.Contains(account.Code)) continue;
            _context.Accounts.Add(new Account
            {
                Code = account.Code,
                NameBn = account.NameBn,
                NameEn = account.NameEn,
                AccountType = account.Type,
                IsSystem = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, Account>> GetAccountsByCodeAsync(CancellationToken cancellationToken = default)
        => await _context.Accounts.AsNoTracking()
            .ToDictionaryAsync(a => a.Code, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

    public async Task<JournalEntry> PostAsync(
        JournalSourceType sourceType,
        Guid? sourceId,
        string? sourceNumber,
        string memo,
        DateTime entryDateUtc,
        IReadOnlyList<JournalLineDraft> lines,
        CurrentUser user,
        string? entryNumber = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count < 2)
            throw new DomainRuleViolationException(DomainErrorCodes.JournalTooFewLines, "A journal entry needs at least two lines.");

        var accounts = await GetAccountsByCodeAsync(cancellationToken).ConfigureAwait(false);

        decimal debits = 0m;
        decimal credits = 0m;
        foreach (var line in lines)
        {
            if (line.Debit < 0m || line.Credit < 0m)
                throw new DomainRuleViolationException(DomainErrorCodes.NegativePrice, "Journal amounts cannot be negative.");
            if (!accounts.ContainsKey(line.AccountCode))
                throw new DomainRuleViolationException(DomainErrorCodes.JournalNotBalanced, $"Unknown account code '{line.AccountCode}'.");
            debits += line.Debit;
            credits += line.Credit;
        }

        debits = MoneyMath.Round(debits);
        credits = MoneyMath.Round(credits);
        if (debits != credits)
        {
            throw new DomainRuleViolationException(
                DomainErrorCodes.JournalNotBalanced,
                $"Journal entry does not balance: debits {debits} vs credits {credits}.");
        }

        if (debits == 0m)
            throw new DomainRuleViolationException(DomainErrorCodes.JournalNotBalanced, "A journal entry cannot be all zero.");

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber ?? await new DocumentNumberService(_context)
                .NextAsync(DocumentNumberService.JournalKey, "JV-{YYYYMM}-{SEQ:5}", entryDateUtc, cancellationToken)
                .ConfigureAwait(false),
            EntryDateUtc = entryDateUtc,
            Memo = memo,
            SourceType = sourceType,
            SourceId = sourceId,
            SourceNumber = sourceNumber,
            PostedAtUtc = DateTime.UtcNow,
            CreatedByUserId = user.UserId
        };

        var ordered = lines
            .Select(l => new { Line = l, Account = accounts[l.AccountCode] })
            .OrderBy(x => x.Account.Code, StringComparer.Ordinal)
            .ToList();

        foreach (var item in ordered)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = item.Account.Id,
                AccountCodeSnapshot = item.Account.Code,
                Debit = MoneyMath.Round(item.Line.Debit),
                Credit = MoneyMath.Round(item.Line.Credit),
                Memo = item.Line.Memo,
                CustomerId = item.Line.CustomerId,
                SupplierId = item.Line.SupplierId
            });
        }

        _context.JournalEntries.Add(entry);
        return entry;
    }
}
