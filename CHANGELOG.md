# Changelog

All notable changes to Tahabil are documented here. Format follows Keep a Changelog;
versioning follows Semantic Versioning.

## [Unreleased]

### Added — 0.1.0 foundation
- Layered solution: `Merqo.Domain`, `Merqo.Infrastructure`, `Merqo.Desktop`, plus two test projects.
- Exact money type (`Money`), documented rounding policy and remainder-preserving allocation.
- `SaleCalculator` — the single implementation of line discount, line VAT, invoice-level discount
  allocation, cash rounding, paid/due/change and per-line cost of goods sold.
- `StockPolicy` with configurable negative-stock override and stock level classification.
- `WeightedAverageCost` valuation with frozen per-movement cost.
- `UnitConversionResolver` with cycle, depth and factor validation.
- SQLite schema (products, categories, brands, units, barcodes, business settings) with WAL,
  `synchronous=FULL`, enforced foreign keys, integer-scaled decimals and UTC timestamps.
- Database initializer with integrity check and system unit seeding (no dummy business data).
- Structured JSON-lines file logger with secret scrubbing and retention.
- WPF shell with light-mode design tokens, Bangla UI and a real, database-backed readiness page.
- CI pipeline on Windows: restore, build, test, self-contained publish, executable smoke launch.

### Added — services and schema (still 0.1.0)
- Full relational schema: 43 tables across parties, sales, purchases, inventory, accounting, security
  and licensing, with `RESTRICT` referential policy and unique document/idempotency keys.
- `SaleService`: one atomic transaction for invoice, items, stock issue at frozen cost, payment,
  customer ledger, balanced journal entry and hash-chained audit record.
- `PurchaseService`: atomic purchase with weighted-average recost, supplier ledger and journal.
- `StockService`, `LedgerService`, `AccountingService` (22-account chart), `DocumentNumberService`
  (gap-free, unique-by-index numbering), `AuditService` (tamper-evident hash chain).
- 7 integration tests covering sales lifecycle, dues, idempotency, stock rejection and frozen COGS.

### Security
- No secrets, keys or customer data are committed; `.gitignore` blocks databases, logs, licences and keys.
