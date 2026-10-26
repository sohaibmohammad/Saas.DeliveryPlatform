using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;

public record GetBranchesQuery(
    Guid BusinessId,
    Guid UserId,
    bool IsAdmin,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<BranchResponse>>;
