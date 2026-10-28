using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetByBusinessIdAndCategoryIdAsync(Guid businessId, Guid categoryId, CancellationToken cancellationToken);
    Task<Category?> GetByBusinessIdAndNameAsync(Guid businessId, string name, CancellationToken cancellationToken);
    Task<List<Category>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);
}
