using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;
using BranchEntity = DeliveryPlatform.Domain.src.Entity.Branch;

namespace DeliveryPlatform.Application.src.Features.Branch.Update;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IGenericRepository<BranchEntity> _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchHandler(
        IBusinessRepository businessRepository,
        IGenericRepository<BranchEntity> branchRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(
            request.BranchId,
            cancellationToken);

        if (branch is null || branch.BusinessId != request.BusinessId)
            throw new KeyNotFoundException("Branch not found.");

        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new UnauthorizedAccessException("You do not own this business.");

        branch.Name = request.Name.Trim();
        branch.Address = request.Address.Trim();
        branch.Latitude = request.Latitude;
        branch.Longitude = request.Longitude;
        branch.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return branch.Id;
    }
}
