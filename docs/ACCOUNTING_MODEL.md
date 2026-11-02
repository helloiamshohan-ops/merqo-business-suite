# Accounting Model

## 1. Principles

1. **Double entry.** Every financial event posts a journal entry whose debits equal credits, enforced
   in the domain (`DomainErrorCodes.JournalNotBalanced`) and re-checked in tests.
2. **Revenue is not cash.** A credit sale increases revenue and receivables; collecting the receivable
   later moves cash and reduces receivables — it is *not* new revenue.
3. **Inventory purchase is not an expense.** Buying stock increases inventory (asset). Cost becomes an
   expense only when the goods are sold (COGS).
4. **Supplier payment is not an expense.** It settles a payable created by a purchase.
5. **History is immutable.** Corrections happen through reversal + re-post, never by editing or
   deleting a posted journal entry. Voided documents keep their number and get status `void`.
6. **Exact money.** All amounts are exact decimals rounded half-away-from-zero to 2 dp, posted as
   scaled integers.

## 2. Profit & loss structure

```
Gross sales (invoice line gross, before discounts)
 − Line discounts
 − Invoice-level discounts (allocated pro-rata across lines by line total)
= Net sales
 − Sales returns (at original selling price)
= Revenue (net)
 − Cost of goods sold (frozen weighted-average cost captured at the moment of sale)
= Gross profit
 − Operating expenses (rent, utilities, salary, transport, repair, misc.)
= Net operating profit
```

Sales VAT/tax is a liability, never revenue. Rounding adjustments are posted to a dedicated
"Rounding difference" account so the till always reconciles to the journal.

## 3. Chart of accounts (initial)

| Code | Account | Type | Normal |
|---|---|---|---|
| 1000 | Cash in hand | Asset | Dr |
| 1010 | Bank / bKash / Nagad | Asset | Dr |
| 1100 | Accounts receivable (customers) | Asset | Dr |
| 1200 | Inventory | Asset | Dr |
| 1500 | Shop equipment | Asset | Dr |
| 2000 | Accounts payable (suppliers) | Liability | Cr |
| 2100 | VAT payable | Liability | Cr |
| 2200 | Customer advance | Liability | Cr |
| 3000 | Owner's capital | Equity | Cr |
| 3100 | Retained earnings | Equity | Cr |
| 4000 | Sales revenue | Revenue | Cr |
| 4010 | Sales returns | Revenue (contra) | Dr |
| 4020 | Sales discounts | Revenue (contra) | Dr |
| 4030 | Rounding difference | Revenue (contra) | Dr/Cr |
| 5000 | Cost of goods sold | Expense | Dr |
| 6000 | Shop rent | Expense | Dr |
| 6010 | Utilities (electricity, water) | Expense | Dr |
| 6020 | Employee salary | Expense | Dr |
| 6030 | Transport | Expense | Dr |
| 6040 | Repair & maintenance | Expense | Dr |
| 6090 | Miscellaneous expense | Expense | Dr |
| 6100 | Inventory shrinkage / adjustment | Expense | Dr |

Expense categories map to 6000-series accounts; the mapping is configurable but defaults exist so a
shop can record an expense on day one.

## 4. Posting rules

**Credit sale** (invoice ৳1,000, COGS ৳700, paid ৳400 cash, VAT ৳0)

| Account | Dr | Cr |
|---|---|---|
| 1000 Cash | 400.00 | |
| 1100 Accounts receivable | 600.00 | |
| 4000 Sales revenue | | 1,000.00 |
| 5000 COGS | 700.00 | |
| 1200 Inventory | | 700.00 |

**Customer due collection ৳600** — Dr Cash 600 / Cr Receivable 600. *No revenue.*

**Purchase on credit** (৳5,000, paid ৳2,000)

| Account | Dr | Cr |
|---|---|---|
| 1200 Inventory | 5,000.00 | |
| 1000 Cash | | 2,000.00 |
| 2000 Accounts payable | | 3,000.00 |

**Operating expense ৳1,500 cash** — Dr 6000-series expense 1,500 / Cr Cash 1,500.

**Sales return** (goods back into stock, refund in cash) — reverse at the *original* revenue and the
*original* frozen cost: Dr 4010 Sales returns / Cr Cash or Receivable at selling price, and
Dr Inventory / Cr COGS at the captured cost. This keeps gross profit after a return identical to
never having sold the item.

**Stock adjustment (loss)** — Dr 6100 Shrinkage / Cr 1200 Inventory at current average cost.

**Supplier payment** — Dr Accounts payable / Cr Cash. *No expense.*

## 5. Inventory valuation

Moving weighted average:
```
newAvg = (qtyOnHand × avgCost + inQty × inUnitCost) / (qtyOnHand + inQty)
```
The average is kept at 4 dp internally and rounded to 2 dp when posted. Each stock-out stores a
frozen `unit_cost` on the movement, so COGS never changes retroactively. Negative stock is blocked by
default (see `StockPolicy`) because it corrupts average cost.

## 6. Reconciliation invariants (asserted by tests)

1. Σ journal debits = Σ journal credits, for every entry and for the whole ledger.
2. Customer receivable balance = Σ invoices − Σ receipts − Σ returns, per customer and in total.
3. Supplier payable balance = Σ purchases − Σ payments − Σ purchase returns.
4. Cash balance = Σ cash movements (opening + inflows − outflows).
5. Inventory value = Σ(qty on hand × average cost) = ledger balance of 1200.
6. Net sales in the P&L = Σ sale line net revenue − Σ sales returns.
7. Gross profit = net sales − COGS, where COGS uses the frozen per-movement cost.

Any report that cannot satisfy its invariant is a bug, not a rounding nuance.
