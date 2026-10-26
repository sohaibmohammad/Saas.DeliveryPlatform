using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.Create;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchHandler(
        IBusinessRepository businessRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new KeyNotFoundException("Business not found.");

        var branch = new DeliveryPlatform.Domain.src.Entity.Branch
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = true
        };

        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return branch.Id;
    }
}
