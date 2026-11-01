using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;

public class GetBranchInventoryHandler : IRequestHandler<GetBranchInventoryQuery, PagedResult<BranchInventoryResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public GetBranchInventoryHandler(
        IBusinessRepository businessRepository,
        IBranchRepository branchRepository,
        IInventoryRepository inventoryRepository)
    {
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<PagedResult<BranchInventoryResponse>> Handle(GetBranchInventoryQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("Business not found.");

        var branch = await _branchRepository.GetByBusinessIdAndBranchIdAsync(
            request.BusinessId,
            request.BranchId,
            cancellationToken);

        if (branch is null)
            throw new NotFoundException("Branch not found.");

        var (inventories, totalCount) = await _inventoryRepository.GetByBranchIdAsync(
            request.BranchId,pageNumber, pageSize,
            cancellationToken);
        

        return new PagedResult<BranchInventoryResponse>
        {
            Items = inventories,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
