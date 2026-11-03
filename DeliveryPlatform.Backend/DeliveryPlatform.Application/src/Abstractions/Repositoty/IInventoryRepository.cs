using DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;
using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IInventoryRepository : IGenericRepository<Inventory>
{
    Task<Inventory?> GetByBranchIdAndProductIdAsync(Guid branchId, Guid productId, CancellationToken cancellationToken);
    Task<(List<BranchInventoryResponse>, int TotalCount)> GetByBranchIdAsync(Guid branchId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<bool> ReserveStockAsync(Guid branchId, Guid productId, int quantityToReserve, CancellationToken cancellationToken);
    Task<bool> SetQuantityAsync(Guid inventoryId, int quantity, CancellationToken cancellationToken);
}
