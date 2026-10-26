using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.Update;

public class UpdateBusinessHandler : IRequestHandler<UpdateBusinessCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBusinessHandler(IBusinessRepository businessRepository, IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Business name is required.");

        if (!Enum.IsDefined(request.BusinessType))
            throw new ArgumentException("Invalid business type.");

        var business = await _businessRepository
            .GetByUserIdAndBusinessIdAsync(
                request.UserId,
                request.BusinessId,
                cancellationToken);

        if (business is null)
            throw new KeyNotFoundException("Business not found.");

        business.Name = request.Name.Trim();

        business.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        business.BusinessType = request.BusinessType;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return business.Id;
    }
}
