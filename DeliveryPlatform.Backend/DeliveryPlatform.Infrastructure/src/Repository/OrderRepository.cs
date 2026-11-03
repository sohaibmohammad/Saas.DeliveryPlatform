using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, totalCount);
    }
}
