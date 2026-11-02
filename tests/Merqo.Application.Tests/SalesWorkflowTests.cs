using FluentAssertions;
using Merqo.Application.Common;
using Merqo.Application.Contracts;
using Merqo.Application.Services;
using Merqo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Merqo.Application.Tests;

/// <summary>Workflow A (sales lifecycle) and Workflow D (accounting) at the service boundary.</summary>
public sealed class SalesWorkflowTests
{
    private static CreateSaleRequest SaleRequest(BusinessFixture f, Guid? customerId, decimal qty, decimal price, decimal paid, string key = "k1", decimal discount = 0m)
        => new(key, customerId, new[] { new SaleLineRequest(f.ProductId, qty, 1m, price, DiscountAmount: discount) }, 0m, f.CashMethodId, paid);

    [Fact]
    public async Task Cash_sale_reduces_stock_records_payment_and_balances_the_journal()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        // Receive stock through a real purchase so average cost is realistic.
        var purchase = await new PurchaseService(context).CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 10m, 1m, 60m) }, 0m, f.CashMethodId, 600m),
            CurrentUser.System);
        purchase.Success.Should().BeTrue();

        var sale = await new SaleService(context).CreateAsync(
            SaleRequest(f, null, 2m, 75m, 150m), CurrentUser.System);

        sale.Success.Should().BeTrue(sale.DeveloperMessage);
        var result = sale.Value!;
        result.GrandTotal.Should().Be(150m);
        result.Due.Should().Be(0m);

        var balance = await context.StockBalances.SingleAsync(b => b.ProductId == f.ProductId);
        balance.QuantityOnHand.Should().Be(8m);

        var reloaded = await context.Sales.Include(s => s.Items).SingleAsync();
        reloaded.InvoiceNumber.Should().NotBeNullOrWhiteSpace();
        reloaded.Items.Single().BaseQuantity.Should().Be(2m);
        reloaded.TotalCost.Should().Be(120m);
        reloaded.GrossProfit.Should().Be(30m);

        (await context.SalePayments.CountAsync()).Should().Be(1);
        (await context.AuditLogs.CountAsync(a => a.Action == "sale.create")).Should().Be(1);
    }

    [Fact]
    public async Task Partial_payment_creates_customer_due_and_ledger_entry()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        await new PurchaseService(context).CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 5m, 1m, 60m) }, 0m, f.CashMethodId, 300m),
            CurrentUser.System);

        var sale = await new SaleService(context).CreateAsync(
            SaleRequest(f, f.CustomerId, 4m, 75m, 100m, "k2"), CurrentUser.System);

        sale.Success.Should().BeTrue(sale.DeveloperMessage);
        sale.Value!.Due.Should().Be(200m);

        var ledger = await new LedgerService(context).GetCustomerBalanceAsync(f.CustomerId);
        ledger.Should().Be(200m);

        var entries = await context.CustomerLedgerEntries.ToListAsync();
        entries.Should().HaveCount(1);
        entries[0].EntryType.Should().Be(LedgerEntryType.Sale);
        entries[0].BalanceAfter.Should().Be(200m);
    }

    [Fact]
    public async Task Sale_beyond_available_stock_is_rejected_and_writes_nothing()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        await new PurchaseService(context).CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 3m, 1m, 60m) }, 0m, f.CashMethodId, 180m),
            CurrentUser.System);

        var sale = await new SaleService(context).CreateAsync(
            SaleRequest(f, null, 5m, 75m, 375m, "k3"), CurrentUser.System);

        sale.Success.Should().BeFalse();
        sale.ErrorCode.Should().Be(Merqo.Domain.Common.DomainErrorCodes.InsufficientStock);

        (await context.Sales.CountAsync()).Should().Be(0);
        (await context.StockMovements.CountAsync(m => m.MovementType == StockMovementType.Sale)).Should().Be(0);
        var balance = await context.StockBalances.SingleAsync(b => b.ProductId == f.ProductId);
        balance.QuantityOnHand.Should().Be(3m);
    }

    [Fact]
    public async Task Duplicate_idempotency_key_never_creates_a_second_invoice()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        await new PurchaseService(context).CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 10m, 1m, 60m) }, 0m, f.CashMethodId, 600m),
            CurrentUser.System);

        var service = new SaleService(context);
        var first = await service.CreateAsync(SaleRequest(f, null, 1m, 75m, 75m, "same-key"), CurrentUser.System);
        var second = await service.CreateAsync(SaleRequest(f, null, 1m, 75m, 75m, "same-key"), CurrentUser.System);

        first.Success.Should().BeTrue();
        second.Success.Should().BeFalse();
        second.ErrorCode.Should().Be(ApplicationErrorCodes.DuplicateSubmission);
        (await context.Sales.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Walk_in_customer_cannot_leave_a_due()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        await new PurchaseService(context).CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 5m, 1m, 60m) }, 0m, f.CashMethodId, 300m),
            CurrentUser.System);

        var sale = await new SaleService(context).CreateAsync(
            SaleRequest(f, null, 1m, 75m, 50m, "walkin"), CurrentUser.System);

        sale.Success.Should().BeFalse();
        sale.ErrorCode.Should().Be(ApplicationErrorCodes.ValidationFailed);
        (await context.Sales.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Cogs_uses_the_cost_frozen_at_sale_time_even_after_a_later_price_rise()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        var purchaseService = new PurchaseService(context);
        await purchaseService.CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 10m, 1m, 50m) }, 0m, f.CashMethodId, 500m),
            CurrentUser.System);

        var sale = await new SaleService(context).CreateAsync(SaleRequest(f, null, 2m, 75m, 150m, "s1"), CurrentUser.System);
        sale.Success.Should().BeTrue(sale.DeveloperMessage);
        sale.Value!.TotalCost.Should().Be(100m);

        // Buy the same product much dearer afterwards; the historical sale must not change.
        await purchaseService.CreateAsync(
            new CreatePurchaseRequest("p2", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 10m, 1m, 90m) }, 0m, f.CashMethodId, 900m),
            CurrentUser.System);

        var stored = await context.Sales.Include(s => s.Items).SingleAsync();
        stored.TotalCost.Should().Be(100m);
        stored.Items.Single().UnitCost.Should().Be(50m);

        var balance = await context.StockBalances.SingleAsync(b => b.ProductId == f.ProductId);
        // 10 bought at 50, 2 sold, 10 bought at 90 -> (8*50 + 900) / 18 = 72.2222
        balance.QuantityOnHand.Should().Be(18m);
        balance.AverageCost.Should().Be(72.2222m);
    }

    [Fact]
    public async Task Every_posted_journal_entry_balances_and_sales_are_numbered_sequentially()
    {
        await using var f = await BusinessFixture.CreateAsync();
        await using var context = f.NewContext();

        var purchaseService = new PurchaseService(context);
        await purchaseService.CreateAsync(
            new CreatePurchaseRequest("p1", f.SupplierId, new[] { new PurchaseLineRequest(f.ProductId, 20m, 1m, 60m) }, 0m, f.CashMethodId, 1000m),
            CurrentUser.System);

        var saleService = new SaleService(context);
        for (int i = 0; i < 3; i++)
        {
            var r = await saleService.CreateAsync(SaleRequest(f, null, 1m, 75m, 75m, $"seq-{i}"), CurrentUser.System);
            r.Success.Should().BeTrue(r.DeveloperMessage);
        }

        var entries = await context.JournalEntries.Include(e => e.Lines).ToListAsync();
        entries.Should().NotBeEmpty();
        foreach (var entry in entries)
        {
            entry.Lines.Sum(l => l.Debit).Should().Be(entry.Lines.Sum(l => l.Credit), $"entry {entry.EntryNumber} must balance");
        }
        entries.Sum(e => e.Lines.Sum(l => l.Debit)).Should().Be(entries.Sum(e => e.Lines.Sum(l => l.Credit)));

        var invoices = await context.Sales.OrderBy(s => s.InvoiceNumber).Select(s => s.InvoiceNumber).ToListAsync();
        invoices.Should().OnlyHaveUniqueItems();
        invoices.Should().HaveCount(3);

        var auditOk = await new AuditService(context).FindFirstTamperedRowAsync();
        auditOk.Should().BeNull();
    }
}
