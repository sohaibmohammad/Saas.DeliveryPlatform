using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Order.Get;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;

public record GetBusinessOrdersQuery(
    Guid BusinessId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    Guid? BranchId = null
) : IRequest<PagedResult<OrderSummaryResponse>>;