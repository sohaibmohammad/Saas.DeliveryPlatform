using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<(List<Order> Orders, int TotalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
