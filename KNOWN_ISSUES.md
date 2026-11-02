# Known Issues and Documented Limitations

Last reviewed: 2026-11-02 (v0.1.0 foundation)

## A. Product is incomplete — read this first

Tahabil is **under construction**. Only the foundation is built: money/calculation engine, SQLite
persistence core, logging, WPF shell, and CI. Business screens (POS, products, purchases, customers,
suppliers, expenses, accounting, reports, printing, licence activation) do **not** exist yet.
`MASTER_CHECKLIST.md` is authoritative and currently records 59 requirements as `NOT_STARTED`.

Any UI in the current build is a readiness screen, not a shop counter.

Backend coverage is further along than the UI: sale and purchase posting (stock, ledgers, journal, audit)
is implemented and covered by integration tests, but there are no product, POS, customer, supplier,
accounting or report screens yet.

## B. External verification limits (not defects, but must not be misrepresented)

| ID | Limitation | Impact | Mitigation |
|---|---|---|---|
| `ENV-L1` | No physical thermal printer, scanner or shop PC available | 58/80 mm receipts are verified by raster/PDF inspection and byte-level ESC/POS tests, never by a real print | documented on-site checklist for the owner |
| `ENV-L2` | No human-observed Windows desktop session | UI verified by automated launch + layout review, not by clicking every control | owner acceptance pass at handover |
| `ENV-L3` | Build sandbox cannot compile .NET; raw Actions logs are network-blocked | iteration happens through CI + diagnostics issue #1 | documented in `docs/ENVIRONMENT_REPORT.md` |
| `ENV-L4` | Installer can only be tested silently on a runner | interactive UAC/locale behaviour unverified | owner test on a clean Windows VM before sale |

## C. Open technical items

| ID | Item | Severity | Status |
|---|---|---|---|
| `TECH-1` | EF migrations not yet created (schema uses `EnsureCreated` until the first migration) | Medium | Open — before data model freeze |
| `TECH-2` | Full business schema modelled and exercised; EF **migrations** still pending | Medium | Partly resolved |
| `TECH-3` | Application services exist for sale/purchase only; the rest of the CRUD surface is still missing | Medium | Open |
| `TECH-4` | Package versions pinned but `CentralPackageFloatingVersionsEnabled` still true; release gate removes it | Low | Open |
| `TECH-5` | QuestPDF is free only below USD 1M annual revenue | Legal | Open — revisit before scaling sales, or replace with a permissively licensed renderer |
| `TECH-6` | Product name "Tahabil" has NOT been trademark-cleared | Legal | Open — owner to commission a search before launch |

## D. Deliberate product decisions

- Light mode only (per brief); no theme switching.
- No cloud sync, no online licence check, no telemetry, no subscription.
- Negative stock is blocked by default; overriding it requires a permission and is audited.
- History is never deleted; documents are voided/reversed, entities archived.
