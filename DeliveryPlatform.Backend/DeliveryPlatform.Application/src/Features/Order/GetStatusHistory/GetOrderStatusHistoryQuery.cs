using MediatR;

namespace DeliveryPlatform.Application.src.Features.Order.GetStatusHistory;

public record GetOrderStatusHistoryQuery(Guid OrderId)
    : IRequest<List<OrderStatusHistorResponse>>;