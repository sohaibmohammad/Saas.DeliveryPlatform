namespace DeliveryPlatform.Application.src.Features.Order.Create;

public record CreateOrderRequest(
    Guid BranchId,
    string DeliveryAddress,
    List<CreateOrderItemRequest> Items
);
