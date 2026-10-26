using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;

public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, PagedResult<BranchResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBranchRepository _branchRepository;

    public GetBranchesHandler(
        IBusinessRepository businessRepository,
        IBranchRepository branchRepository)
    {
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
    }

    public async Task<PagedResult<BranchResponse>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var business = request.IsAdmin
            ? await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken)
            : await _businessRepository.GetByUserIdAndBusinessIdAsync(
                request.UserId,
                request.BusinessId,
                cancellationToken);

        if (business is null)
            throw new KeyNotFoundException("Business not found.");

        var branches = await _branchRepository.GetByBusinessIdAsync(
            request.BusinessId,
            cancellationToken);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var items = branches
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BranchResponse(
                b.Id,
                b.BusinessId,
                b.Name,
                b.Address,
                b.Latitude,
                b.Longitude,
                b.IsActive))
            .ToList();

        return new PagedResult<BranchResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = branches.Count
        };
    }
}
