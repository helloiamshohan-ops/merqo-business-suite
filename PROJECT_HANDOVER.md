# Project Handover / Session Recovery Brief

Read this first if you are resuming work on Tahabil.

## 1. Ten-second summary

`Tahabil` (MERQO) is a Bangla-first, offline Windows business suite for Bangladeshi shops.
A layered .NET 10 solution is in place with a **verified calculation engine, full relational schema,
atomic sale/purchase posting services** (stock + ledgers + double-entry journal + hash-chained audit),
a Bangla WPF shell and a Windows CI pipeline that builds, tests, publishes and launches the real
executable (51 tests green). The **business screens are not built yet** — see `MASTER_CHECKLIST.md`.

## 2. Repository facts

| Item | Value |
|---|---|
| Repo | `helloiamshohan-ops/merqo-business-suite` (public) |
| Default branch | `main` |
| Working branch | `arena/fce4c4bd-merqo-business-suite` |
| Solution | `Merqo.slnx` (slnx format) |
| SDK | .NET 10.0.403 (`global.json`) |
| Diagnostics | GitHub issue **#1** — every CI run posts its logs there |

## 3. How to build and verify

Nothing builds in the agent sandbox (no .NET SDK, no Windows). **All verification happens in CI:**

```pwsh
# locally on a Windows machine
dotnet restore Merqo.slnx
dotnet build   Merqo.slnx -c Release
dotnet test    Merqo.slnx -c Release

# in the sandbox: trigger the canonical verification instead
git push origin arena/fce4c4bd-merqo-business-suite
gh run list --limit 1
gh issue view 1 --json comments --jq '.comments[-1].body'   # read the CI log
```

## 4. Project layout

```
src/Merqo.Domain          entities + pure business rules (no EF, no WPF)  [+ 35 tests]
src/Merqo.Infrastructure  EF Core/SQLite, paths, logging, diagnostics     [+ 9 tests]
src/Merqo.Desktop         WPF app (assembly name Tahabil) + MVVM shell
src/Merqo.Application     use cases: sale/purchase/stock/ledger/journal/audit  [+ 7 tests]
tests/                    xunit suites
docs/                     all design and process documents
.github/workflows/ci.yml  Windows build → test → publish → smoke launch → artefacts
```

## 5. Conventions you must not break

1. **No formula in the UI.** Money/stock/ledger math lives in `Merqo.Domain` only.
2. **No floating point for money.** `Money`/`MoneyMath` + integer-scaled storage.
3. **One transaction per business event**, and printing strictly after commit.
4. **Never delete financial history.** Archive, void or reverse.
5. **Permissions in services**, not merely hidden buttons.
6. **Bangla for users, English for developers.** Domain throws stable codes; the UI maps to Bangla.
7. **Every claim in the docs must be reproducible by a CI run or a test.**

## 6. Immediate next steps

1. EF migrations (replace `EnsureCreated`) and pre-migration backup.
2. Remaining services: payment collection, returns, expenses, stock adjustment, void/reversal.
3. Authentication + RBAC + session/permission service.
4. Screens: settings → products/catalogue → POS → sales history → parties → purchases → expenses →
   returns → stock adjustment → accounting → dashboard → reports.
5. Printing, exports, backup/restore, licensing, installer, release.

## 7. Traps discovered so far (save yourself the CI cycles)

- `actions/checkout` **deletes untracked folders** — create log folders after checkout.
- Central package management rejects floating versions unless
  `CentralPackageFloatingVersionsEnabled` is set (it is, temporarily, while pinning).
- EF's `SqlQuery` cannot run `PRAGMA` statements (not composable) — use `SqliteDiagnostics` (plain ADO).
- `DateTimeStyles.RoundtripKind` cannot be combined with `AdjustToUniversal`.
- A namespace named `...Money` collides with a type named `Money` — the finance types live in
  `Merqo.Domain.Finance`.
- WPF's implicit usings do **not** include `System.IO`.
- PowerShell: a bare `*>&1 | Tee-Object` line is a syntax error and fails the step.
