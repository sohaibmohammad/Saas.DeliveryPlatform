using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Category?> GetByBusinessIdAndCategoryIdAsync(Guid businessId, Guid categoryId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == categoryId, cancellationToken);
    }

    public async Task<Category?> GetByBusinessIdAndNameAsync(Guid businessId, string name, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Name == name, cancellationToken);
    }

    public async Task<List<Category>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(c => c.BusinessId == businessId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
