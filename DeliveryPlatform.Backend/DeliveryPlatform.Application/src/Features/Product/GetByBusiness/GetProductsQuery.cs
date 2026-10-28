using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.GetByBusiness;

public record GetProductsQuery(
    Guid BusinessId,
    Guid UserId,
    bool IsAdmin,
    Guid? CategoryId = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<ProductResponse>>;
