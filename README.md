# তাহাবিল (Tahabil) — MERQO Business Suite

**তাহাবিল** is an offline, lifetime-licensed Windows desktop business management suite for
Bangladeshi retail and wholesale shops. It covers point of sale, sales, purchases, inventory,
customers, suppliers, dues, expenses, double-entry accounting, returns, reports, thermal receipt
printing and backup/restore — entirely on the shop's own computer, with no internet dependency.

| | |
|---|---|
| Product | **তাহাবিল (Tahabil)** |
| Tagline | বিক্রয়, মজুত আর হিসাব — এক জায়গায় |
| Brand | MERQO |
| Founder | Shohan Khan |
| Contact | merqoonline@gmail.com |
| Version | 0.1.0 (foundation — see `DEVELOPMENT_STATUS.md`) |
| Platform | Windows 10 / 11 (x64) |
| Licence model | One-time payment, offline lifetime activation |

> **Status honesty:** this repository is under active construction. `DEVELOPMENT_STATUS.md` is the
> single source of truth for what is implemented, tested and verified. Nothing is marked *verified*
> until it has actually run — see `MASTER_CHECKLIST.md`.

## Documentation map

| Document | Purpose |
|---|---|
| `DEVELOPMENT_STATUS.md` | Current phase, verified modules, next tasks |
| `MASTER_CHECKLIST.md` | Every requirement with an ID and evidence-based status |
| `PROJECT_HANDOVER.md` | Session-recovery brief |
| `KNOWN_ISSUES.md` | Open defects and documented limitations |
| `docs/ENVIRONMENT_REPORT.md` | Phase 0 environment capabilities and limitations |
| `docs/PRODUCT_BLUEPRINT.md` | Product, market, workflows, scope |
| `docs/TECH_STACK.md` | Stack selection and justification |
| `docs/SYSTEM_ARCHITECTURE.md` | Layering and module design |
| `docs/DATABASE_SCHEMA.md` | Schema, keys, indexes, precision, deletion policy |
| `docs/ACCOUNTING_MODEL.md` | Chart of accounts and posting rules |
| `docs/LICENSE_ARCHITECTURE.md` | Signing, binding, recovery, threat model |
| `docs/PRINTING_ARCHITECTURE.md` | Bengali thermal receipts and PDF |
| `docs/SECURITY_ARCHITECTURE.md` | Authentication, RBAC, audit, key handling |
| `docs/BUILD_AND_RELEASE_PLAN.md` | CI/CD, packaging, release gates |
| `docs/INSTALLATION_GUIDE.md` | Install, upgrade, uninstall, data safety |
| `docs/USER_MANUAL_BN.md` | বাংলা ব্যবহার নির্দেশিকা |
| `docs/TESTING_STRATEGY.md` | Test pyramid and mandatory workflows |
| `docs/QA_REPORT.md` | Executed test evidence |
| `docs/RELEASE_NOTES.md` | Release notes |
| `docs/RELEASE_GATES.md` | Release approval checklist |

## Building

Requires the .NET SDK pinned in `global.json` and Windows for the WPF projects.

```powershell
dotnet restore Merqo.slnx
dotnet build Merqo.slnx -c Release
dotnet test Merqo.slnx -c Release
dotnet publish src/Merqo.Desktop/Merqo.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The Windows release pipeline lives in `.github/workflows/`. Every artefact published by CI is built
from a tagged commit and accompanied by SHA-256 checksums.
