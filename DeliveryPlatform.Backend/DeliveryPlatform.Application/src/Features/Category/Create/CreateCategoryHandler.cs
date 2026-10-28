using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.Create;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryHandler(
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("Business not found.");

        var existing = await _categoryRepository.GetByBusinessIdAndNameAsync(
            request.BusinessId,
            request.Name.Trim(),
            cancellationToken);

        if (existing is not null)
            throw new BusinessRuleException("Category with the same name already exists for this business.");

        var category = new DeliveryPlatform.Domain.src.Entity.Category
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = request.Name.Trim()
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
