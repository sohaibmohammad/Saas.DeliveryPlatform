using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.Get;

public record GetMyOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderSummaryResponse>>;
