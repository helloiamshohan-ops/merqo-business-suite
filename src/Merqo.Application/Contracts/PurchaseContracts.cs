namespace Merqo.Application.Contracts;

public sealed record PurchaseLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitFactorToBase,
    decimal UnitCost,
    decimal DiscountAmount = 0m,
    decimal TaxRatePercent = 0m);

public sealed record CreatePurchaseRequest(
    string IdempotencyKey,
    Guid SupplierId,
    IReadOnlyList<PurchaseLineRequest> Lines,
    decimal InvoiceDiscountAmount,
    Guid PaymentMethodId,
    decimal PaidAmount,
    string? Note = null);

public sealed record PurchaseCreatedResult(
    Guid PurchaseId,
    string PurchaseNumber,
    decimal GrandTotal,
    decimal Paid,
    decimal Due);
