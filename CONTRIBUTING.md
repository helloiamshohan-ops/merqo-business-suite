# Contributing

Tahabil is a commercial product owned by MERQO. These rules keep the codebase releasable.

## Ground rules

1. Never commit secrets, signing keys, customer data, databases or build output.
2. Every visible control must be wired to real behaviour. No placeholder screens, no fake numbers.
3. Business arithmetic lives in the domain layer only. UI code must never re-implement a formula.
4. Financial tables are append-only and corrected by reversal, never by silent deletion.
5. A change is not "done" until it has tests and the tests have actually run in CI.

## Workflow

- Branch: `feature/<area>-<short-name>`, `fix/<area>-<short-name>`.
- Commit style: `feat:`, `fix:`, `test:`, `docs:`, `build:`, `security:`, `refactor:`.
- Run `dotnet build Merqo.slnx -c Release` and `dotnet test Merqo.slnx -c Release` before pushing.
- Update `DEVELOPMENT_STATUS.md`, `MASTER_CHECKLIST.md` and `KNOWN_ISSUES.md` with the change.

## Code style

- C# with nullable reference types enabled; no `!` suppression without a comment justifying it.
- Domain types express invariants; invalid states must be unrepresentable where practical.
- Bengali user-facing strings live in the presentation layer, mapped from stable error codes.
