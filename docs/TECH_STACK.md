# Technology Stack — Selection and Justification

Selected: **2026-11-02**. Everything below is chosen for a *lifetime-licensed, entirely offline,
Bangla-first Windows desktop product for Bangladeshi shops*, not for developer familiarity.

## 1. Candidates compared

| Option | Strengths | Why it was rejected / chosen |
|---|---|---|
| **C# + .NET 10 + WPF + SQLite + EF Core** | Native Windows; single self-contained EXE (no installer of a runtime); WPF has real text shaping via DirectWrite (correct Bengali conjuncts, kars, reph, hasanta); excellent printing interop; strong test ecosystem; permissive tooling | **CHOSEN** |
| Electron + SQLite + React | Fast UI iteration, web skills | 150–250 MB installer, 200–400 MB RAM at idle, Chromium security patching for years after a one-time-payment sale, and no native print-quality control for ESC/POS. Poor fit for low-spec shop PCs and a lifetime licence. |
| Python + PySide6 + SQLite | Quick to write | Packaging to a reliable Windows EXE (PyInstaller) yields false-positive antivirus flags, slow cold start, and painful Windows printing APIs. Lifetime maintenance risk. |
| Java + JavaFX | Cross-platform | Runtime bootstrapping, heavier memory, weaker Windows printing/ESC-POS story. |
| Delphi / C++ Qt | Fast, native | Commercial tooling cost and a tiny talent pool for future maintenance of a product sold once. |
| Tauri + Rust + WebView2 | Tiny binaries | Depends on the WebView2 runtime the shop PC may lack; Rust UI iteration for a large business app is slow; complex printing path. |

## 2. Selected stack

| Concern | Choice | Version policy | Reason |
|---|---|---|---|
| Runtime | .NET 10 LTS (`net10.0`, `net10.0-windows`) | pinned `global.json` | LTS to 2028 supports the lifetime-licence promise; .NET 8 ends 2026-11-10 |
| UI | WPF, MVVM via `CommunityToolkit.Mvvm` | `8.*` → pinned | First-class Bangla text shaping through DirectWrite; hardware-accelerated tables; mature |
| Database | SQLite (`Microsoft.Data.Sqlite` + `Microsoft.EntityFrameworkCore.Sqlite`) | `10.0.*` → pinned | Single-file, ACID, WAL, zero administration, backup by snapshot API |
| Data access | EF Core for CRUD + hand-written parameterised SQL for reporting aggregates | — | EF for correctness, SQL for report speed over scaled-integer columns |
| Money | `decimal` in the domain, **integer minor units** in SQLite | — | Exact arithmetic; no binary floating point anywhere in financial paths |
| Password hashing | PBKDF2-HMAC-SHA256, 210 000 iterations, 128-bit salt (see note) | — | Built into the base class library: no third-party crypto dependency to audit or patch |
| Licensing crypto | **ECDSA P-256 / SHA-256** signatures over a canonical JSON payload | — | In-box, mature, FIPS 186-4, deterministic-key format, far less code to get wrong than a hand-rolled Ed25519. Documented in `LICENSE_ARCHITECTURE.md` with the Ed25519 trade-off explained |
| PDF | QuestPDF | `2025.*` → pinned | Skia/HarfBuzz shaping gives correct Bengali in PDF; layout engine suits reports and receipts |
| Thermal receipt | SkiaSharp raster → ESC/POS `GS v 0` + Windows spooler fallback | `3.*` → pinned | The only reliable way to print Bengali on 58/80 mm printers; avoids code-page traps |
| XLSX export | ClosedXML | `0.104.*` → pinned | MIT, writes real cell types, handles Bangla text |
| CSV export | Hand-written writer | — | UTF-8 BOM control belongs to us, not a library |
| Logging | In-house JSON-lines file logger | — | Zero dependencies, guaranteed secret scrubbing, daily rotation + retention |
| Tests | xunit + FluentAssertions **7.x** | — | FluentAssertions 8+ moved to a paid licence → deliberately held at 7.x |
| Installer | Inno Setup 6 | — | Free, scriptable, supports Bengali strings, proper uninstall and data preservation |
| CI/CD | GitHub Actions on `windows-latest` | — | Only environment in this project that can actually build and run Windows binaries |

## 3. Third-party licence review

| Package | Licence | Commercial distribution impact |
|---|---|---|
| EF Core / Microsoft.Data.Sqlite / Extensions | MIT | None |
| CommunityToolkit.Mvvm | MIT | None |
| xunit, FluentAssertions 7.x | Apache-2.0 / Apache-2.0 | None (FluentAssertions 8+ avoided) |
| QuestPDF | Community licence | Free below USD 1M annual revenue; **must be revisited (or replaced) before selling above that threshold** — tracked in `KNOWN_ISSUES.md` |
| SkiaSharp (+HarfBuzz) | MIT | None |
| ClosedXML | MIT | None |
| Inno Setup | Free (Inno Setup licence) | None |
| Noto Sans Bengali | SIL OFL 1.1 | Redistributable with attribution |

## 4. Deliberate exclusions

- No cloud database, no remote authentication, no online licence check, no telemetry.
- No paid SDKs, no subscription services, no required internet access after activation.
- No dark mode / theming engine (single light design system, per brief).
