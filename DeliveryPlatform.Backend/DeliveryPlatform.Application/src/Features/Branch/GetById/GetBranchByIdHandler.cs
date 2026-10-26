using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.GetById;

public class GetBranchByIdHandler : IRequestHandler<GetBranchByIdQuery, BranchResponse?>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBranchRepository _branchRepository;

    public GetBranchByIdHandler(
        IBusinessRepository businessRepository,
        IBranchRepository branchRepository)
    {
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
    }

    public async Task<BranchResponse?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var business = request.IsAdmin
            ? await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken)
            : await _businessRepository.GetByUserIdAndBusinessIdAsync(
                request.UserId,
                request.BusinessId,
                cancellationToken);

        if (business is null)
            return null;

        var branch = await _branchRepository.GetByBusinessIdAndBranchIdAsync(
            request.BusinessId,
            request.BranchId,
            cancellationToken);

        if (branch is null)
            return null;

        return new BranchResponse(
            branch.Id,
            branch.BusinessId,
            branch.Name,
            branch.Address,
            branch.Latitude,
            branch.Longitude,
            branch.IsActive);
    }
}
