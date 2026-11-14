using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IOrderRepository : IGenericRepository<Order>
{
    void AddStatusHistory(OrderStatusHistory history);
    Task<PagedResult<Order>> GetByBusinessIdPagedAsync(Guid businessId, Guid? branchId, OrderStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Order?> GetByIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<(List<Order> Orders, int TotalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResult<Order>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Order?> GetForCancellationAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<Order?> GetOrderDetailsAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<Order?> GetOrderForStatusUpdateAsync(Guid orderId, CancellationToken cancellationToken);
    Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Order?> GetOrderWithStatusHistoryForUserAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task<bool> TryUpdateStatusAsync(Guid orderId, OrderStatus expectedStatus, OrderStatus newStatus, CancellationToken cancellationToken);
}
