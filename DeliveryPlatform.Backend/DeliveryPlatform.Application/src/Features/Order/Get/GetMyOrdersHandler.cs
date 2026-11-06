using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.Get;

public class GetMyOrdersHandler
    : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderSummaryResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyOrdersHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<OrderSummaryResponse>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var page=request.PageNumber < 1 ? 1 : request.PageNumber;
        var size = Math.Clamp(request.PageSize, 1, 50);
        var userId = _currentUserService.UserId;

        var result =await _orderRepository.GetByUserIdPagedAsync(userId, page, size, cancellationToken);
        return new PagedResult<OrderSummaryResponse>
        {
            Items = result.Items.Select(order => new OrderSummaryResponse(
                order.Id,
                order.OrderNumber,
                order.Status,
                order.Subtotal,
                order.DeliveryFee,
                order.Total,
                order.DeliveryAddress,
                order.CreatedAt
            )).ToList(),

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}