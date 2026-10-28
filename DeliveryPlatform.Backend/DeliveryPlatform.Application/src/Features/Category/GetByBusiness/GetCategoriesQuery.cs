using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.GetByBusiness;

public record GetCategoriesQuery(
    Guid BusinessId,
    Guid UserId,
    bool IsAdmin,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<CategoryResponse>>;
