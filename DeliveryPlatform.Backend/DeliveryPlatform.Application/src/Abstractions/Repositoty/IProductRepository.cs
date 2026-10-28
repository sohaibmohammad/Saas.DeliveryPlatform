using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByBusinessIdAndProductIdAsync(Guid businessId, Guid productId, CancellationToken cancellationToken);
    Task<List<Product>> GetByBusinessIdAsync(Guid businessId, Guid? categoryId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameInCategoryAsync(Guid businessId, Guid categoryId, string name, Guid? excludeProductId, CancellationToken cancellationToken);
}
