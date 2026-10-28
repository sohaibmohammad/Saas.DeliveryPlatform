using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Product?> GetByBusinessIdAndProductIdAsync(Guid businessId, Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == productId, cancellationToken);
    }

    public async Task<List<Product>> GetByBusinessIdAsync(Guid businessId, Guid? categoryId, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.BusinessId == businessId);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameInCategoryAsync(Guid businessId, Guid categoryId, string name, Guid? excludeProductId, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Where(p => p.BusinessId == businessId && p.CategoryId == categoryId && p.Name == name);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
