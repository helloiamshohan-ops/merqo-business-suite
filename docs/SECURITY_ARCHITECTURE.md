# Security Architecture

## 1. Authentication

- Local accounts only; no remote identity provider.
- Passwords hashed with **PBKDF2-HMAC-SHA256, 210 000 iterations, 128-bit random salt, 256-bit hash**,
  stored as `iterations$salt$hash` with algorithm metadata so parameters can be raised later without
  invalidating existing hashes.
- Policy: minimum 8 characters, mixed classes encouraged (not enforced destructively), no maximum
  length, no silent truncation.
- Failed logins are rate-limited per account (exponential backoff after 5 failures) and recorded in
  `login_attempts` and the audit log. Lockout is temporary, never destructive.
- Sessions are in-process; the first-run owner account must change its initial password immediately.

## 2. Authorisation

Granular permission strings (`sales.create`, `sales.view`, `sales.return`, `products.manage`,
`products.price.edit`, `stock.adjust`, `purchases.manage`, `payments.record`, `customers.manage`,
`suppliers.manage`, `reports.view`, `reports.export`, `expenses.manage`, `database.restore`,
`users.manage`, `settings.manage`, `license.view`, `audit.view`).

Roles seeded: Owner, Administrator, Manager, Cashier, Inventory Manager, Accountant. Owner is a
superset and cannot be locked out by accident (at least one owner must always remain active).

Enforcement points: **application services**, via `[RequiresPermission]`-style checks and a single
`IAuthorizationService`. Hiding a button is a convenience, never the security boundary.

## 3. Audit log

Append-only from the application's perspective: inserts only, no update/delete path in code. Records
user, action, entity, entity id, timestamp (UTC), before/after deltas for sensitive fields, and
success/failure. Covered events: login/logout/failed login, product create/modify/archive, sale,
sale return, purchase, purchase return, payment, expense, stock adjustment, price change, user and
permission changes, settings changes, backup, restore, licence activation, restore of a deleted record.

Tamper resistance: `audit_logs` has no UI edit path, and each row carries a hash chain
(`previous_hash` → `row_hash`) so silent hand-editing of the database is detectable.

## 4. Data protection on disk

- Business data sits under `%LOCALAPPDATA%\MERQO\Tahabil` (per-user ACL by default).
- Licence private keys never exist on a customer machine.
- Logs scrub `password`, `passwd`, `pwd`, `secret`, `token`, `pin`, `private key` values (implemented
  and unit-tested in `FileAppLogger`).
- Backups are plain SQLite files by design (a shop must be able to recover without the owner); the UI
  warns that the backup folder should be treated as confidential and supports a USB destination.

## 5. Injection and input safety

- All SQL parameterised; report queries use parameters only. No string concatenation of user input.
- EF Core LINQ for entity access; raw SQL restricted to report aggregates with typed parameters.
- Input length limits enforced at DTO boundary; HTML/format strings never interpreted.

## 6. Application hardening

- Single instance per data folder (mutex) so two processes cannot interleave writes.
- No stack traces shown to users; technical detail goes to the log with a correlation id.
- Restore and destructive maintenance actions require an owner permission and explicit confirmation.
- Update mechanism: none automatic. Distribution is owner-controlled; no self-updating code path
  exists that could be hijacked.
