using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.Create;

public class CreateBusinessHandler : IRequestHandler<CreateBusinessCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBusinessHandler(IBusinessRepository businessRepository, IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Business name is required.");
        }
        if (!Enum.IsDefined(request.BusinessType))
        {
            throw new ArgumentException("Invalid business type.");
        }

        var business = new DeliveryPlatform.Domain.src.Entity.Business
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            BusinessType = request.BusinessType,
            UserId = request.UserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _businessRepository.AddAsync(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return business.Id;
    }
}
