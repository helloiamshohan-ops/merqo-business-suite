# Printing Architecture (Bengali thermal receipts, PDF)

## 1. The core problem

A 58 mm/80 mm ESC/POS thermal printer accepts a single legacy code page. Bengali is not in any of
them, and no consumer thermal printer shapes Bengali conjuncts (যুক্তাক্ষর), kars, reph or hasanta.
Solution: **render the receipt to a raster image with a real text-shaping engine, then send the bitmap
to the printer** with ESC/POS `GS v 0` raster commands. If the printer cannot take raster data, fall
back to printing the same image through the Windows spooler.

## 2. Pipeline

```
Receipt model (invoice, items, totals, business profile)
        │
        ├─ Layout engine  → logical lines & columns for the configured paper width
        │        (58 mm ≈ 32 chars / 384 dots, 80 mm ≈ 48 chars / 576 dots at 203 dpi)
        │
        ├─ SkiaSharp + HarfBuzz shaping → 1-bit bitmap (correct Bengali shaping, embedded
        │        Noto Sans Bengali + a Latin fallback, manual bidi for mixed text)
        │
        └─ Transport
                 ├─ ESC/POS raster (raw bytes via Win32 `spoolss` / `winspool.drv`)
                 ├─ Windows spooler (PrintDocument of the bitmap)
                 └─ PDF (QuestPDF, fonts embedded) for archiving / WhatsApp / email
```

## 3. Layout rules

- All money right-aligned; quantities right-aligned; names left-aligned with ellipsis at the column edge.
- Bangla numerals optional (setting); English numerals default for thermal legibility.
- Item name wraps to at most two lines at 58 mm, preserving whole words (Bangla words are split on
  spaces only — never inside a grapheme cluster).
- Totals block: subtotal, discounts, VAT, grand total (bold, larger), paid, due/change.
- Header: business name, address, phone, invoice number, date/time, cashier, customer.
- Footer: configurable Bangla text, return policy, thank-you line.
- Copies: configurable (1–3), `Bill`/`Copy` marker on duplicates; reprints are marked `পুনর্মুদ্রণ`.

## 4. Isolation from business logic

Printing is invoked **after** the sale transaction commits, from a queue with a bounded retry. Failures
are surfaced ("প্রিন্টার পাওয়া যায়নি — বিল সংরক্ষিত হয়েছে, পুনরায় প্রিন্ট করুন") and never roll back
or duplicate a sale. Reprints are always possible from Sales History.

## 5. Fonts

- `Noto Sans Bengali` (SIL OFL 1.1) embedded as WPF resource + copied for the raster engine;
  attribution kept in `assets/fonts/OFL.txt`.
- Verified rendering set: যুক্তাক্ষর (ক্ষ, জ্ঞ, ঞ্চ), কার (ি ী ু ূ ৃ ে ৈ ো ৌ), ফলা (্য ্র), রেফ (র্),
  হসন্ত (্), বাংলা সংখ্যা, English text, and mixed Bangla-English in one line.
- Vertical metrics: line height = ascent + descent + 2 px leading, so no clipping of descenders or
  hasanta marks; a golden-image test asserts no clipped pixel rows at the top/bottom of the text box.

## 6. Verification reality

No physical printer exists in the build environment. Verification therefore consists of:
1. byte-level ESC/POS tests (raster header, width bytes, termination),
2. golden bitmap images compared to approved snapshots,
3. PDF page render and inspection,
4. a documented on-site checklist the owner runs once on each supported printer model.

The product never claims "printer verified" from a build alone; the distinction is recorded in
`MASTER_CHECKLIST.md`.
