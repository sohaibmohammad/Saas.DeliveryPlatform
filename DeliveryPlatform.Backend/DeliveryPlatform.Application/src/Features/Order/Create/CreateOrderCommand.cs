using MediatR;

namespace DeliveryPlatform.Application.src.Features.Order.Create;

public record CreateOrderCommand(
    Guid BranchId,
    string DeliveryAddress,
    List<CreateOrderItemRequest> Items
) : IRequest<CreateOrderResponse>;
