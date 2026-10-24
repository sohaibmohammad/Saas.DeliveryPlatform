using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IBusinessRepository : IGenericRepository<Business>
{
    Task<List<Business>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Business?> GetByUserIdAndBusinessIdAsync(Guid userId, Guid businessId, CancellationToken cancellationToken);
}
