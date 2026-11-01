using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByBranchIdAndProductIdAsync(Guid branchId, Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(
                i => i.BranchId == branchId && i.ProductId == productId,
                cancellationToken);
    }

    public async Task<(List<BranchInventoryResponse>, int TotalCount)> GetByBranchIdAsync(Guid branchId,int pageNumber,int pageSize, CancellationToken cancellationToken)
    {

        var query = _context.Inventories.AsNoTracking().Where(i => i.BranchId == branchId)
            ;
        var totalCount = await query.CountAsync(cancellationToken);


        var inventories = await query.Include(i => i.Product)
            .OrderBy(i => i.Product.Name).ThenBy(i => i.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)
  .Select(i => new BranchInventoryResponse(
            i.ProductId,
            i.Product.Name,
            i.Quantity,
            i.ReservedQuantity,
            i.Quantity - i.ReservedQuantity))
        .ToListAsync(cancellationToken);

        return (inventories, totalCount);
    }

    public async Task<bool> SetQuantityAsync(Guid inventoryId, int quantity, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.Id == inventoryId, cancellationToken);

        if (inventory is null || quantity < inventory.ReservedQuantity)
            return false;

        inventory.Quantity = quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
