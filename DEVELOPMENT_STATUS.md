# Development Status

**Last updated:** 2026-11-02
**Branch:** `arena/fce4c4bd-merqo-business-suite`
**Version:** 0.1.0 (foundation)
**Phase:** Phase 3 — Project Foundation (in progress). Phases 0–2 complete.

## 1. What actually works today (verified)

| Capability | Evidence |
|---|---|
| Windows CI builds, tests, publishes and launches the real EXE | run `41528496087`: build ✅, test ✅ (35 + 9), publish ✅ |
| 44 automated tests green (35 domain, 9 SQLite integration) | `dotnet test` output in diagnostics issue #1 |
| Self-contained single-file `Tahabil.exe` produced | CI publish step, `publish/desktop/Tahabil.exe` |
| SQLite database created at startup with WAL + FK + FULL sync | `DatabaseTests.Wal_mode_and_foreign_keys_are_active` |
| Exact decimal storage (integer-scaled), UTC timestamps | `DatabaseTests.Decimal_values_roundtrip_exactly...` |
| Constraint enforcement (unique barcode/code, FK, rollback) | 4 database tests |
| Sale arithmetic incl. pro-rata discount allocation, VAT, cash rounding | 13 `SaleCalculatorTests` |
| Unit conversion with cycle detection | 5 `UnitConversionTests` |
| Stock policy + weighted average costing | 7 `InventoryTests` |
| Bangla WPF shell with design tokens | `Merqo.Desktop`, UI render smoke step |
| Secret-scrubbing structured logging | `FileAppLogger` |

## 2. What does NOT exist yet (do not demo it)

POS, product/category screens, inventory screens, purchases, customers, suppliers, expenses,
accounting, reports, exports, thermal printing, licence activation, installer, user management.
`MASTER_CHECKLIST.md` lists 59 requirements as `NOT_STARTED` and is the source of truth.

## 3. Test counts

| Suite | Tests | Status |
|---|---|---|
| `Merqo.Domain.Tests` | 35 | passing |
| `Merqo.Infrastructure.Tests` | 9 | passing |
| Total | 44 | passing (run `41528496087`) |

## 4. Next exact tasks (in order)

1. Complete the full business schema as EF migrations (sales, purchases, ledgers, journals, audit,
   licence tables) — `TECH-1`, `TECH-2`.
2. Add the `Merqo.Application` layer: unit of work, permission checks, audit writer, sale/purchase/
   payment services with real transaction boundaries.
3. Implement business settings + user/role/permission management + authentication.
4. Product/category/brand/unit screens; then inventory, POS, sales, customers, suppliers, purchases,
   expenses, returns, accounting, dashboard, reports.
5. Printing (raster Bengali receipts), exports (PDF/XLSX/CSV), backup/restore.
6. Licence generator + offline activation.
7. Installer, release workflow, QA report, Bangla user manual.

## 5. Session-recovery state

- Current commit: see `git log -1` on the branch (every milestone is committed and pushed).
- Working tree is kept clean; CI must be green before moving to the next milestone.
- Latest CI run and its logs: issue **#1** in the repository (`CI diagnostics feed`).
- Known blockers: none technical. External limits are listed in `KNOWN_ISSUES.md` §B.

## 6. Architectural decisions locked in

- .NET 10 LTS + WPF + EF Core + SQLite; money stored as scaled integers.
- Print-after-commit, never inside the transaction.
- History is immutable: archive/void/reverse, never delete.
- Application services (not UI) enforce permissions and own transactions.
- Licence: ECDSA P-256 signature verified offline; private key never in repo, EXE or CI.
