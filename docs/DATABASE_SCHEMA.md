# Database Schema

Engine: SQLite (via `Microsoft.Data.Sqlite` + EF Core 10).
File: `%LOCALAPPDATA%\MERQO\Tahabil\tahabil.db`.

## 1. Engine configuration (verified by tests)

| Setting | Value | Why |
|---|---|---|
| `journal_mode` | `WAL` | readers never block the till during a sale |
| `synchronous` | `FULL` | acknowledged sale survives a power cut |
| `foreign_keys` | `ON` | referential integrity is enforced, not advisory |
| `busy_timeout` | 8000 ms | backup thread vs. UI writes |
| `temp_store` | `MEMORY` | report sorting on low-end PCs |
| connection | `Pooling=true`, `ReadWriteCreate` | fast startup, no OS-level lock surprises |

## 2. Storage conventions

| Concern | Representation | Rationale |
|---|---|---|
| Money | `INTEGER`, scaled ×10⁴ (poisha ×100 within it) | exact, `SUM`-able in SQL, no float |
| Quantity / rate / factor | `INTEGER`, scaled ×10⁴ | exact, sortable, aggregatable |
| Timestamps | `TEXT` UTC ISO-8601 `yyyy-MM-ddTHH:mm:ss.fffffffZ` | lexicographic ordering == chronological ordering |
| Booleans | `INTEGER` 0/1 | SQLite native |
| Ids | `TEXT` GUID (UUIDv4, application-generated) | offline creation without coordination |
| Names | `snake_case` for tables, columns, keys, indexes | stable hand-written reporting SQL |
| Soft delete | `is_archived` + `archived_at_utc` with EF query filter | history is never destroyed |

`decimal` in the domain maps to `long` scaled storage through `DecimalToScaledLongConverter`.
Range: ±9.2 × 10¹⁴ at 4 dp — far beyond any shop's lifetime turnover.

## 3. Implemented tables (v0.1 foundation)

| Table | Purpose | Key constraints |
|---|---|---|
| `categories` | product grouping | unique `name_bn`; archive flag |
| `brands` | product brands | unique `name_bn`; archive flag |
| `units` | units of measure with hierarchy | unique `name_bn`; FK `base_unit_id` → `units` (RESTRICT) |
| `products` | product master | unique `code`; index `name_bn`; FK `category_id`, `brand_id`, `unit_id` (RESTRICT) |
| `product_barcodes` | many barcodes per product | **unique `barcode`**; FK `product_id` CASCADE |
| `business_settings` | single business profile row | singleton by application rule |
| `application_settings` | key/value UI settings | unique `key` |
| `__EFMigrationsHistory` | migration ledger | EF-managed |

Delete policy on `products`: **never hard-deleted**. Archive only. Every FK from a transaction table
to `products` is `RESTRICT`, so a product that appears in any historical document cannot vanish.

## 4. Planned tables (full schema, next milestones)

Grouped exactly as required by the brief:

- **Configuration**: `business_settings`, `app_settings`, `currencies`, `tax_settings`,
  `invoice_settings`, `printer_settings`, `backup_settings`, `document_sequences`
- **Security**: `users`, `roles`, `permissions`, `role_permissions`, `user_roles`, `user_sessions`,
  `login_attempts`, `audit_logs`
- **Catalogue/inventory**: `products`, `product_variants`, `product_barcodes`, `categories`,
  `brands`, `units`, `unit_conversions`, `warehouses`, `stock_balances`, `stock_movements`,
  `stock_adjustments`, `stock_adjustment_items`
- **Parties**: `customers`, `customer_addresses`, `suppliers`, `supplier_contacts`,
  `customer_ledger_entries`, `supplier_ledger_entries`
- **Sales**: `sales`, `sale_items`, `sale_item_discounts`, `sale_payments`, `sale_returns`,
  `sale_return_items`, `invoices`, `invoice_sequences`
- **Purchases**: `purchases`, `purchase_items`, `purchase_payments`, `purchase_returns`,
  `purchase_return_items`
- **Cash**: `payment_methods`, `payments`, `payment_allocations`, `cash_accounts`,
  `cash_movements`, `cash_sessions`
- **Accounting**: `expense_categories`, `expenses`, `chart_of_accounts`, `accounting_periods`,
  `journal_entries`, `journal_entry_lines`, `financial_adjustments`
- **Operations**: `report_presets`, `backup_history`, `application_logs`, `notifications`,
  `migration_history`
- **Licensing**: `license_records`, `activation_records`, `machine_bindings`,
  `license_recovery_records`

## 5. Migration and backup strategy

- Schema changes ship as EF migrations; `DatabaseInitializer` applies them on startup and records the
  applied set in `__EFMigrationsHistory`.
- A pre-migration file copy is taken automatically before applying a migration to a non-empty database.
- Backups use the SQLite **online backup API** (not a file copy), so WAL content is included and the
  copy is consistent while the application is running.
- Restore = verify → automatic pre-restore backup → atomic replace → reopen → integrity check →
  audit entry. The application never deletes the current database before a verified copy exists.
- `PRAGMA integrity_check` runs at startup and before/after every backup and restore.
