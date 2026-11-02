namespace Merqo.Application.Contracts;

public sealed record SaleLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitFactorToBase,
    decimal UnitPrice,
    decimal DiscountAmount = 0m,
    decimal TaxRatePercent = 0m);

public sealed record CreateSaleRequest(
    string IdempotencyKey,
    Guid? CustomerId,
    IReadOnlyList<SaleLineRequest> Lines,
    decimal InvoiceDiscountAmount,
    Guid PaymentMethodId,
    decimal PaidAmount,
    string? Note = null,
    bool? RoundGrandTotalToTaka = null);

public sealed record SaleCreatedResult(
    Guid SaleId,
    string InvoiceNumber,
    decimal GrandTotal,
    decimal Paid,
    decimal Due,
    decimal Change,
    decimal NetSales,
    decimal TotalCost,
    decimal GrossProfit);
