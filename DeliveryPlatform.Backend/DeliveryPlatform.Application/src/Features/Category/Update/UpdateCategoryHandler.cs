using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.Update;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryHandler(
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("Business not found.");

        var category = await _categoryRepository.GetByBusinessIdAndCategoryIdAsync(
            request.BusinessId,
            request.CategoryId,
            cancellationToken);

        if (category is null)
            throw new NotFoundException("Category not found.");

        var existing = await _categoryRepository.GetByBusinessIdAndNameAsync(
            request.BusinessId,
            request.Name.Trim(),
            cancellationToken);

        if (existing is not null && existing.Id != category.Id)
            throw new BusinessRuleException("Category with the same name already exists for this business.");

        category.Name = request.Name.Trim();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
