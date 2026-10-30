using DeliveryPlatform.Application.src.Abstractions.Repositoty;
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

    public async Task<List<Inventory>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.BranchId == branchId)
            .OrderBy(i => i.Product.Name)
            .ToListAsync(cancellationToken);
    }
}
