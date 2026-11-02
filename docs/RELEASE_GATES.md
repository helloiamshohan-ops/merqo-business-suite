# Release Gates and Owner Authorisation

A release may only be published when every line below is true **and** the owner has authorised it.

## Preconditions

- [ ] `dotnet build Merqo.slnx -c Release` clean
- [ ] `dotnet test Merqo.slnx -c Release` — all green
- [ ] Published `Tahabil.exe` launches on the Windows runner and creates its database
- [ ] Installer builds and installs silently on the runner, shortcuts present, uninstall preserves data
- [ ] `TahabilLicenseGenerator.exe` builds and contains **no** key material
- [ ] No floating package versions
- [ ] No secrets, `.db`, logs, keys or build output in the repository
- [ ] `MASTER_CHECKLIST.md` evidence current; no `VERIFIED` row without a run reference
- [ ] `KNOWN_ISSUES.md` reviewed; no critical/high defect undisclosed
- [ ] `CHANGELOG.md`, `docs/RELEASE_NOTES.md`, `VERSION` all agree on the version
- [ ] Backup/restore exercised end to end
- [ ] Accounting invariants (journal balanced, ledgers reconciled) proven by tests

## Owner authorisation

Publishing a release is an irreversible, public action. It is performed only on the owner's explicit
instruction in-session. The pipeline creates a **draft** release; a human clicks publish.

## Record

| Field | Value |
|---|---|
| Version | — |
| Commit | — |
| CI run | — |
| Authorised by | — |
| Date | — |
