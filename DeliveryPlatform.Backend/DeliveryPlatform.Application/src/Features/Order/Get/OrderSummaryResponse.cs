using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Features.Order.Get;

public record OrderSummaryResponse(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Total,
    string DeliveryAddress,
    DateTime CreatedAt
);