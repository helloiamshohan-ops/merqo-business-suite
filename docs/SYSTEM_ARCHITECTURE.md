# System Architecture

## 1. Layers (dependency rule: inward only)

```
┌──────────────────────────────────────────────────────────────┐
│ Merqo.Desktop  (WPF, MVVM)      presentation, Bangla strings │
│   Views / ViewModels / Commands / Dialogs / Print preview    │
├──────────────────────────────────────────────────────────────┤
│ Merqo.Application (planned)     use cases, authorisation,    │
│   SaleService, PurchaseService, PaymentService, ReportService│
│   ICurrentUser, IPermissionService, IUnitOfWork, DTOs        │
├──────────────────────────────────────────────────────────────┤
│ Merqo.Domain                    entities + pure rules       │
│   Money, MoneyMath, SaleCalculator, StockPolicy,             │
│   WeightedAverageCost, UnitConversionResolver,               │
│   journal rules, error codes                                 │
├──────────────────────────────────────────────────────────────┤
│ Merqo.Infrastructure            persistence, crypto, files   │
│   TahabilDbContext, converters, PRAGMAs, logger, backup,     │
│   printing transports, export engines, licence verifier      │
└──────────────────────────────────────────────────────────────┘
                      SQLite file in %LOCALAPPDATA%
```

Rules enforced by review and by the test suite:
1. **No financial formula in the UI.** A `ViewModel` may only call a domain/application service.
2. **No SQL in the UI.**
3. Domain code has no dependency on EF Core, WPF, or the file system, so it is unit-testable in
   milliseconds (currently 35 tests in ~100 ms).
4. Application services own the transaction boundary and the permission check, and emit audit events.

## 2. Cross-cutting services

| Service | Responsibility | Current state |
|---|---|---|
| `AppPaths` | Every mutable path (db, logs, backups, licence, exports) | implemented |
| `IAppLogger` / `FileAppLogger` | JSON-lines, daily rolling, secret scrubbing, retention | implemented |
| `DatabaseInitializer` | create/migrate, seed system reference data, integrity check | implemented |
| `SqlitePragmaInterceptor` | WAL, `synchronous=FULL`, foreign keys, busy timeout | implemented |
| Error handling | stable error codes → Bangla messages; no stack traces shown to users | partial (shell) |
| Licensing gate | verifies signed licence before business screens open | planned |
| Backup engine | SQLite online backup + verification | planned |

## 3. Transaction design

A committed business event is one `BEGIN IMMEDIATE` transaction covering every related row:
document header, lines, stock movements, cost snapshots, ledger entries, journal entry + lines,
sequence increments and the audit record. `SaveChanges` outside such a transaction is only allowed
for pure reference-data edits.

Printing, PDF generation and export run **after** commit, on a separate code path. A printer error is
reported to the user and never undoes or duplicates a sale. Checkout uses an idempotency token so a
double-click cannot produce two invoices.

## 4. Concurrency and durability

- Single-user desktop, but the UI thread, a background backup thread and a report query may overlap.
- WAL lets readers proceed during a write; `busy_timeout=8000` serialises writers gracefully.
- `synchronous=FULL` is chosen deliberately for a market with frequent power cuts: an acknowledged
  sale must survive an immediate outage.
- Optimistic concurrency (`Version` column) guards reference-data edits against lost updates.

## 5. Reporting strategy

EF Core is used for entity CRUD. Aggregated reports use hand-written parameterised SQL over
scaled-integer columns, because EF cannot translate `SUM` over value-converted decimals. Report
definitions and their reconciliation identities are documented in `docs/ACCOUNTING_MODEL.md`, and
every financial report is cross-checked by an integration test against the journal.

## 6. Packaging topology

```
%LOCALAPPDATA%\MERQO\Tahabil\      ← business data (survives uninstall/upgrade)
    tahabil.db (+ -wal, -shm)
    backups\  logs\  license\  exports\  documents\
C:\Program Files\MERQO\Tahabil\    ← binaries only (Tahabil.exe, deps, icon)
```
Uninstall removes binaries and asks about data; it never silently deletes a shop's books.
