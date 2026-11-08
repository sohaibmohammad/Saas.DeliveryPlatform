using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Order.Get;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;

public class GetBusinessOrdersHandler : IRequestHandler<GetBusinessOrdersQuery, PagedResult<OrderSummaryResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBusinessOrdersHandler(
        IOrderRepository orderRepository,
        IBusinessRepository businessRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _businessRepository = businessRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<OrderSummaryResponse>> Handle(GetBusinessOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var business =await _businessRepository.GetByUserIdAndBusinessIdAsync(userId, request.BusinessId, cancellationToken);

        if (business == null)
            throw new UnauthorizedAccessException(
                "You are not authorized to access this business.");

        var result = await _orderRepository.GetByBusinessIdPagedAsync(
       request.BusinessId,
       request.Status,
       request.PageNumber,
       request.PageSize,
       cancellationToken);

        return new PagedResult<OrderSummaryResponse>
        {
            Items = result.Items.Select(o => new OrderSummaryResponse(
                o.Id,
                o.OrderNumber,
                o.Status,
                o.Subtotal,
                o.DeliveryFee,
                o.Total,
                o.DeliveryAddress,
                o.CreatedAt
            )).ToList(),

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
