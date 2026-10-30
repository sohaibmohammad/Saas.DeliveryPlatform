using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    Task<Inventory?> GetByBranchIdAndProductIdAsync(Guid branchId, Guid productId, CancellationToken cancellationToken);
    Task<List<Inventory>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
}
