using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.Update;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Guid>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
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

    public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
            request.UserId,
            request.BusinessId,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("Business not found.");

        var product = await _productRepository.GetByBusinessIdAndProductIdAsync(
            request.BusinessId,
            request.ProductId,
            cancellationToken);

        if (product is null)
            throw new NotFoundException("Product not found.");

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
            product.Id,
            cancellationToken);

        if (exists)
            throw new BusinessRuleException("Product with the same name already exists in this category.");

        product.CategoryId = category.Id;
        product.Name = request.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        product.Price = request.Price;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
