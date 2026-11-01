using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;

public record GetBranchInventoryQuery(
    Guid BusinessId,
    Guid BranchId,
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<BranchInventoryResponse>>;
