using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class BranchRepository : GenericRepository<Branch>, IBranchRepository
{
    private readonly AppDbContext _context;

    public BranchRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Branch>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken)
    {
        return await _context.Branches
            .Where(b => b.BusinessId == businessId)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Branch?> GetByBusinessIdAndBranchIdAsync(Guid businessId, Guid branchId, CancellationToken cancellationToken)
    {
        return await _context.Branches
            .FirstOrDefaultAsync(
                b => b.BusinessId == businessId && b.Id == branchId,
                cancellationToken);
    }
}
