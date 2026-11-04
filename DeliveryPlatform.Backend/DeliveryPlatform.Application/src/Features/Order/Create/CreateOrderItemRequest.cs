namespace DeliveryPlatform.Application.src.Features.Order.Create;

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity
);
