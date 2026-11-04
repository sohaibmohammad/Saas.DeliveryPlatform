using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Features.Order.Create;

public record CreateOrderResponse(
    Guid OrderId,
    string OrderNumber,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Total,
    OrderStatus Status
);
