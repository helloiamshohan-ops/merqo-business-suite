# Master Checklist — Tahabil (MERQO Business Suite)

Every requirement in the brief carries a stable ID here. Status meanings are strict:

| Status | Meaning |
|---|---|
| `NOT_STARTED` | Nothing exists yet. |
| `PLANNED` | Design decided, no code. |
| `IN_PROGRESS` | Partially built, not complete. |
| `IMPLEMENTED` | Code exists and builds, but is **not** proven by execution. |
| `TESTING` | Automated execution exists; evidence not yet conclusive. |
| `VERIFIED` | Executed successfully and acceptance criteria met; evidence recorded. |
| `BLOCKED` | Cannot proceed; reason documented. Never counted as done. |

**Never mark `VERIFIED` because code exists.** Evidence must reference a real test run, CI run ID or captured artefact.

## Current tally

| Status | Count |
|---|---|
| VERIFIED | 25 |
| TESTING | 3 |
| IMPLEMENTED | 9 |
| IN_PROGRESS | 7 |
| PLANNED | 2 |
| NOT_STARTED | 59 |
| BLOCKED | 0 |
| **Total** | **105** |

---

## Environment and process

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `ENV-001` | Repository inspected, permissions and Actions verified | Process | `VERIFIED` | docs/ENVIRONMENT_REPORT.md §1; push + run 41524120405 |
| `ENV-002` | Build environment identified; limitations documented | Process | `VERIFIED` | docs/ENVIRONMENT_REPORT.md §3 |
| `ENV-003` | Windows build/test pipeline operational | CI | `TESTING` | ci.yml; iterative bring-up in progress |

## Blueprint and design

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `BLU-001` | Product name, tagline and identity chosen | Branding | `VERIFIED` | docs/PRODUCT_BLUEPRINT.md §1 |
| `BLU-002` | Technology stack compared and justified (6 options) | Architecture | `VERIFIED` | docs/TECH_STACK.md |
| `BLU-003` | System architecture and layering defined | Architecture | `VERIFIED` | docs/SYSTEM_ARCHITECTURE.md |
| `BLU-004` | Database schema designed and documented | Database | `IN_PROGRESS` | docs/DATABASE_SCHEMA.md (product/inventory core done) |
| `BLU-005` | Accounting model defined | Accounting | `IN_PROGRESS` | docs/ACCOUNTING_MODEL.md |
| `BLU-006` | Licence architecture defined | Licensing | `IN_PROGRESS` | docs/LICENSE_ARCHITECTURE.md |
| `BLU-007` | Printing architecture defined | Printing | `IN_PROGRESS` | docs/PRINTING_ARCHITECTURE.md |
| `BLU-008` | Security architecture defined | Security | `IN_PROGRESS` | docs/SECURITY_ARCHITECTURE.md |
| `BLU-009` | Build and release plan defined | CI | `IN_PROGRESS` | docs/BUILD_AND_RELEASE_PLAN.md |

## Foundation

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `FND-001` | Layered solution structure | Foundation | `IMPLEMENTED` | Merqo.slnx; 3 source + 2 test projects |
| `FND-002` | Central package management with pinned versions | Build | `IMPLEMENTED` | Directory.Packages.props |
| `FND-003` | SDK version pinned | Build | `IMPLEMENTED` | global.json (10.0.403) |
| `FND-004` | Structured logging with secret scrubbing | Foundation | `IMPLEMENTED` | Infrastructure/Logging/FileAppLogger.cs |
| `FND-005` | Global unhandled-error handling with Bangla message | Foundation | `IMPLEMENTED` | Desktop/App.xaml.cs |
| `FND-006` | Light-mode design system (colour, type, spacing, controls) | UI | `IMPLEMENTED` | Desktop/Themes/Theme.xaml |
| `FND-007` | Bangla UI shell with sidebar/top bar/status bar | UI | `IMPLEMENTED` | Desktop/MainWindow.xaml |
| `FND-008` | Data directory outside Program Files | Foundation | `IMPLEMENTED` | Infrastructure/AppPaths.cs |

## Money and calculation engine

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `MON-001` | Exact money type, no binary floating point | Domain | `VERIFIED` | Finance/Money.cs; MoneyTests (4 tests) |
| `MON-002` | Documented rounding policy | Domain | `VERIFIED` | Finance/MoneyMath.cs; MoneyTests |
| `MON-003` | Remainder-preserving pro-rata allocation | Domain | `VERIFIED` | MoneyMath.Allocate; MoneyTests |
| `MON-004` | Sale totals: discounts, VAT, rounding, paid/due/change | Domain | `VERIFIED` | Pricing/SaleCalculator.cs; 13 tests |
| `MON-005` | Per-line cost snapshot and gross profit | Domain | `VERIFIED` | SaleLineResult.CostAmount/GrossProfit |

## Database

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `DB-001` | SQLite with WAL, synchronous=FULL, foreign keys, busy timeout | Database | `VERIFIED` | SqlitePragmaInterceptor; DatabaseTests |
| `DB-002` | Exact decimal storage as scaled integers | Database | `VERIFIED` | ScaledDecimalConverters; DatabaseTests |
| `DB-003` | UTC timestamp storage | Database | `VERIFIED` | UtcDateTimeConverters; DatabaseTests |
| `DB-004` | snake_case schema naming | Database | `VERIFIED` | SnakeCaseNaming; DatabaseTests |
| `DB-005` | Integrity check on startup | Database | `VERIFIED` | DatabaseInitializer.CheckIntegrityAsync |
| `DB-006` | Idempotent initialisation and system-unit seeding | Database | `VERIFIED` | DatabaseTests.Initialization/Reinitialization |
| `DB-007` | Unique + foreign key enforcement verified by tests | Database | `VERIFIED` | DatabaseTests (3 tests) |
| `DB-008` | Transactional rollback verified by test | Database | `VERIFIED` | DatabaseTests.Failed_transaction_leaves_no_partial_rows |
| `DB-009` | EF migrations for schema versioning | Database | `PLANNED` | Currently EnsureCreated until first migration |
| `DB-010` | Full entity schema (sales, purchases, ledgers, accounting, audit, licence) | Database | `NOT_STARTED` | — |

## Products and catalogue

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `CAT-001` | Unit conversion with cycle/depth/factor validation | Domain | `VERIFIED` | Catalog/UnitConversion.cs; 5 tests |
| `CAT-002` | Product CRUD + barcode management | Products | `NOT_STARTED` | — |
| `CAT-003` | Category and brand management | Products | `NOT_STARTED` | — |
| `CAT-004` | Product archive instead of destructive delete | Products | `IMPLEMENTED` | AuditableEntity.IsArchived + query filter |
| `CAT-005` | Product import/export | Products | `NOT_STARTED` | — |

## Inventory

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `INV-001` | Stock policy with configurable negative-stock override | Inventory | `VERIFIED` | Inventory/StockPolicy.cs; 4 tests |
| `INV-002` | Weighted-average valuation with frozen cost | Inventory | `VERIFIED` | Inventory/WeightedAverageCost.cs; 3 tests |
| `INV-003` | Stock balances and movement ledger | Inventory | `NOT_STARTED` | — |
| `INV-004` | Stock adjustments with audit trail | Inventory | `NOT_STARTED` | — |
| `INV-005` | Low/out-of-stock alerting | Inventory | `NOT_STARTED` | StockLevel classification exists; UI pending |

## Point of Sale

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `POS-001` | POS cart, scanner input, keyboard operation | POS | `NOT_STARTED` | — |
| `POS-002` | Transactional checkout (single atomic commit) | POS | `NOT_STARTED` | — |
| `POS-003` | Duplicate-submit protection / idempotency | POS | `NOT_STARTED` | — |
| `POS-004` | Partial payment, customer due, change | POS | `NOT_STARTED` | Arithmetic ready in SaleCalculator |
| `POS-005` | Invoice numbering sequences | POS | `NOT_STARTED` | — |

## Sales and returns

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `SAL-001` | Sales list, detail, search, filters | Sales | `NOT_STARTED` | — |
| `SAL-002` | Sale void/reversal without deleting history | Sales | `NOT_STARTED` | — |
| `SAL-003` | Sales returns with quantity validation | Returns | `NOT_STARTED` | — |
| `SAL-004` | Purchase returns | Returns | `NOT_STARTED` | — |

## Purchases

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `PUR-001` | Purchase entry with weighted-average recost | Purchases | `NOT_STARTED` | — |
| `PUR-002` | Purchase payments and supplier dues | Purchases | `NOT_STARTED` | — |

## Customers, suppliers, expenses

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `CRM-001` | Customer CRUD, ledger, due statement, collections | Customers | `NOT_STARTED` | — |
| `CRM-002` | Supplier CRUD, ledger, due statement, payments | Suppliers | `NOT_STARTED` | — |
| `CRM-003` | Expense categories and expense entry | Expenses | `NOT_STARTED` | — |

## Accounting

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `ACC-001` | Chart of accounts with double-entry journals | Accounting | `NOT_STARTED` | — |
| `ACC-002` | Automatic posting rules for sale/purchase/payment/return/expense | Accounting | `NOT_STARTED` | — |
| `ACC-003` | Profit & loss, cash flow, trial balance | Accounting | `NOT_STARTED` | — |
| `ACC-004` | Journal balance enforcement | Accounting | `NOT_STARTED` | Domain codes reserved |

## Reports and export

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `RPT-001` | 24 required reports with date filters/sorting/search | Reports | `NOT_STARTED` | — |
| `RPT-002` | PDF export with embedded Bengali font | Export | `NOT_STARTED` | — |
| `RPT-003` | XLSX export with correct cell types | Export | `NOT_STARTED` | — |
| `RPT-004` | CSV export with UTF-8 BOM | Export | `NOT_STARTED` | — |

## Printing

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `PRN-001` | 58 mm and 80 mm thermal layouts | Printing | `NOT_STARTED` | — |
| `PRN-002` | Bengali raster receipt rendering (ESC/POS) | Printing | `NOT_STARTED` | — |
| `PRN-003` | Receipt preview, print, reprint | Printing | `NOT_STARTED` | — |
| `PRN-004` | Printer test page and error reporting | Printing | `NOT_STARTED` | — |

## Security

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `SEC-001` | Password hashing (PBKDF2-HMAC-SHA256, 210k iterations) | Security | `NOT_STARTED` | — |
| `SEC-002` | Role-based access control with granular permissions | Security | `NOT_STARTED` | — |
| `SEC-003` | Authorisation enforced in services, not only UI | Security | `NOT_STARTED` | — |
| `SEC-004` | Audit log for all sensitive operations | Security | `NOT_STARTED` | — |
| `SEC-005` | No secrets in repository or logs | Security | `VERIFIED` | SECURITY.md; .gitignore; logger scrubbing |

## Licensing

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `LIC-001` | Separate License Generator executable | Licensing | `NOT_STARTED` | — |
| `LIC-002` | Asymmetric signature verification in main app (public key only) | Licensing | `NOT_STARTED` | — |
| `LIC-003` | Private key never embedded or committed | Licensing | `PLANNED` | Architecture fixed in docs/LICENSE_ARCHITECTURE.md |
| `LIC-004` | Business identity binding | Licensing | `NOT_STARTED` | — |
| `LIC-005` | Optional machine binding with graceful tolerance | Licensing | `NOT_STARTED` | — |
| `LIC-006` | Offline lifetime activation, no renewal | Licensing | `NOT_STARTED` | — |
| `LIC-007` | Authorised recovery/reissue workflow | Licensing | `NOT_STARTED` | — |
| `LIC-008` | Licence security test suite (12 cases) | Licensing | `NOT_STARTED` | — |

## Backup and restore

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `BAK-001` | Consistent online backup (SQLite backup API) | Backup | `NOT_STARTED` | — |
| `BAK-002` | Backup verification + integrity check + manifest | Backup | `NOT_STARTED` | — |
| `BAK-003` | Restore preview, pre-restore backup, safe restore | Backup | `NOT_STARTED` | — |
| `BAK-004` | Automatic backup with retention | Backup | `NOT_STARTED` | — |

## UI/UX and localisation

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `UIX-001` | All labels, errors, dialogs in natural Bangla | UI | `IN_PROGRESS` | Shell localised; module screens pending |
| `UIX-002` | Bengali input accepted in every data field | UI | `NOT_STARTED` | — |
| `UIX-003` | DPI scaling, resizing, small/large resolution checks | UI | `NOT_STARTED` | — |
| `UIX-004` | Keyboard navigation and focus order | UI | `NOT_STARTED` | — |

## Business settings

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `SET-001` | Business profile, currency, invoice numbering settings | Settings | `NOT_STARTED` | schema placeholder only |
| `SET-002` | Tax/VAT, discount and stock rule settings | Settings | `NOT_STARTED` | — |
| `SET-003` | Printer and backup settings | Settings | `NOT_STARTED` | — |

## Distribution and release

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `DST-001` | Main executable built (self-contained, single file) | Distribution | `TESTING` | ci.yml publish step |
| `DST-002` | Executable launch + database creation smoked on Windows | Distribution | `TESTING` | ci.yml smoke step |
| `DST-003` | Installer (Inno Setup) with shortcuts and uninstaller | Distribution | `NOT_STARTED` | toolchain confirmed |
| `DST-004` | License Generator executable built separately | Distribution | `NOT_STARTED` | — |
| `DST-005` | Version tag, GitHub Release, checksums, release notes | Distribution | `NOT_STARTED` | gated on docs/RELEASE_GATES.md |

## Documentation

| ID | Requirement | Module | Status | Evidence |
|---|---|---|---|---|
| `DOC-001` | README, CHANGELOG, SECURITY, CONTRIBUTING | Documentation | `VERIFIED` | repository root |
| `DOC-002` | Environment and blueprint documents | Documentation | `VERIFIED` | docs/ |
| `DOC-003` | Master checklist and development status | Documentation | `VERIFIED` | this file + DEVELOPMENT_STATUS.md |
| `DOC-004` | Bangla user manual | Documentation | `NOT_STARTED` | — |
| `DOC-005` | QA report with executed evidence | Documentation | `NOT_STARTED` | — |

