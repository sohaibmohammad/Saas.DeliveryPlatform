using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetStatusHistory;

public class GetOrderStatusHistoryHandler : IRequestHandler<GetOrderStatusHistoryQuery, List<OrderStatusHistorResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderStatusHistoryHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<OrderStatusHistorResponse>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            throw new InvalidOperationException("Invalid order ID.");

        var userId = _currentUserService.UserId;

        var order = await _orderRepository.GetOrderWithStatusHistoryForUserAsync(
            request.OrderId,
            userId,
            cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        return order.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistorResponse
            {
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            })
            .ToList();
    }
}
