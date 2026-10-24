using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class BusinessRepository : GenericRepository<Business>, IBusinessRepository
{
    private readonly AppDbContext _context;

    public BusinessRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Business>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Businesses
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new Business
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                BusinessType = b.BusinessType,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UserId = b.UserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Business?> GetByUserIdAndBusinessIdAsync(Guid userId, Guid businessId, CancellationToken cancellationToken)
    {
        return await _context.Businesses
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == businessId, cancellationToken);
    }
}
