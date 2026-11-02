# Product Blueprint

## 1. Product identity

| Field | Value |
|---|---|
| Product name | **তাহাবিল** (Tahabil) |
| Tagline (bn) | বিক্রয়, মজুত আর হিসাব — এক জায়গায় |
| Tagline (en) | Sales, stock and accounts — in one place |
| Brand | MERQO (MERQO is the publisher; Tahabil is the product's own identity) |
| Founder | Shohan Khan |
| Contact | merqoonline@gmail.com |
| Executable | `Tahabil.exe` (main), `TahabilLicenseGenerator.exe` (owner-only tool) |
| Versioning | SemVer, `MAJOR.MINOR.PATCH`, artefacts carry the build commit SHA |

### Why "Tahabil"
- *তাহাবিল* (treasury/funds) is instantly understood by a Bangladeshi shop owner, easy to pronounce
  in Bangla and English, short, and works as a Windows application and installer name.
- It reads as a product, not as a person's name, and does not collide with the well-known Bangla
  marketplaces and mobile-financial-service brands in this space.
- **Not trademark-cleared.** This is an informal screen only: no formal trademark search (BPTO/DPDT
  or WIPO) has been performed. The owner must commission one before commercial launch; this is
  recorded in `KNOWN_ISSUES.md`.

## 2. Target market and fit

Bangladeshi retail/wholesale shops (grocery, hardware, pharmacy-adjacent general stores, wholesale
arot), typically:
- One counter, 1–4 staff, one computer (often modest: 4 GB RAM, HDD, 1366×768, Windows 10/11).
- Intermittent or no internet; unreliable mains power.
- Almost all business communication in Bangla; product names entered in Bangla, some in English.
- A 58 mm or 80 mm thermal receipt printer, often a USB keyboard-wedge barcode scanner.
- Payment on delivery of software is common → **one-time payment, lifetime licence, offline**.

Design consequences: no cloud dependency, fast cold start, small memory footprint, WAL + full
synchronous durability (power cuts), Bangla-first UI and receipts, keyboard-only POS operation,
single-file backup to a USB drive.

## 3. Scope (mandatory modules)

Dashboard · Point of Sale · Sales & invoices · Sales returns · Purchases · Purchase returns ·
Products, categories, brands, units with conversion · Inventory (balances, movements, adjustments,
valuation, alerts) · Customers & customer dues · Suppliers & supplier dues · Payments & collections ·
Expenses · Double-entry accounting, P&L, cash flow · 24 report types with PDF/CSV/XLSX export ·
58/80 mm thermal receipts with Bengali raster rendering · Barcode/QR support · Users, roles,
granular permissions · Audit log · Backup & restore with verification · Offline lifetime licensing
with a separate owner-only License Generator · Business settings · About · Installer · CI/CD release.

## 4. Core workflows

**A. Sale (POS).** Scan/search → cart → quantity & discount → customer (or walk-in) → payment →
commit → print. Commit is one database transaction: invoice, items, stock movement, cost snapshot,
customer ledger, journal entry, audit. Printing happens *after* commit and can never roll it back or
duplicate it.

**B. Purchase.** Supplier → items → paid/partial → commit: purchase, items, stock in with weighted
average recost, supplier ledger, journal, audit.

**C. Due collection.** Customer/supplier → outstanding statement → payment → allocation → ledger →
cash movement → journal.

**D. Returns.** Original invoice → line → return qty (validated against previously returned qty) →
restock decision → refund/credit → journal reversal at original cost snapshot.

**E. Accounting.** Every financial event posts a balanced journal entry; P&L derives from revenue,
COGS, returns, discounts and operating expenses — never from "sales minus purchases".

**F. Backup.** SQLite online-backup API → integrity-checked archive → verified copy → manifest with
version, checksum and row counts; restore is preceded by an automatic pre-restore backup.

## 5. Locale and currency rules

- Currency BDT, symbol ৳, 2 decimals, exact decimal arithmetic, optional whole-taka cash rounding.
- Bangla UI and Bangla numerals in receipts/reports where configured; English digits allowed.
- Dates: `dd/MM/yyyy` default (configurable); storage is always UTC ISO-8601.
- All input fields accept Bangla and English text — nothing is English-only.

## 6. Out of scope (explicit)

Cloud sync, multi-branch replication, mobile apps, payroll, e-commerce integration, dark mode,
subscription licensing, online activation servers.
