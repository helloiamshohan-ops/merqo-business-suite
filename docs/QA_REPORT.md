# QA Report

Evidence-only. Every line references a run that actually happened.

## Run log

| Date | Commit | Workflow | Result | Evidence |
|---|---|---|---|---|
| 2026-11-02 | `feat(app): atomic sale and purchase services…` | `ci` | **success** — build, 51 tests, publish, EXE launch, UI render | run in `gh run list`; logs in issue #1 |
| 2026-11-02 | `fix: add EF using in application test fixture` | `ci` | **success** | idem |

> Earlier failing runs are kept in the run history on purpose; each one is a defect that was fixed,
> not a result that was hidden.

## Automated suites

| Suite | Focus | Count |
|---|---|---|
| `Merqo.Domain.Tests` | money, rounding, allocation, sale arithmetic, unit conversion, stock policy, weighted average cost | 35 |
| `Merqo.Infrastructure.Tests` | PRAGMAs, decimal storage, UTC timestamps, unique/FK/rollback integrity, archive semantics, integrity check | 9 |
| `Merqo.Application.Tests` | atomic sale, partial payment + due ledger, insufficient stock rejection, idempotency, walk-in due rejection, frozen COGS after price change, journal balance + sequential numbering + audit chain | 7 |

## What the pipeline proves about the real executable

1. `dotnet publish` produces a **self-contained single-file `Tahabil.exe`** (no runtime install needed).
2. That executable is **launched on Windows**, creates its SQLite database and stays alive.
3. A second, headless launch with `--render-ui` **composes and lays out the real WPF window** and writes
   a PNG, which is uploaded as a build artefact.

## Not yet covered (honest gaps)

- No UI interaction tests (no automation of clicks); the shell has minimal behaviour to test.
- No printing, PDF/XLSX export, backup/restore, or licensing tests — those features do not exist yet.
- No performance/load testing yet.
- No installer yet, so install/uninstall verification has not run.
