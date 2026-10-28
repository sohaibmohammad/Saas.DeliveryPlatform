using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.Create;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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

        var exists = await _productRepository.ExistsByNameInCategoryAsync(
            request.BusinessId,
            request.CategoryId,
            request.Name.Trim(),
            null,
            cancellationToken);

        if (exists)
            throw new BusinessRuleException("Product with the same name already exists in this category.");

        var product = new DeliveryPlatform.Domain.src.Entity.Product
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            CategoryId = category.Id,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
