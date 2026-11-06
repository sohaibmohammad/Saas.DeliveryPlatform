using MediatR;

namespace DeliveryPlatform.Application.src.Features.Order.GetDetails;

public record GetOrderDetailsQuery(
    Guid OrderId
) : IRequest<OrderDetailsResponse>;