using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Paging;
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
    public async Task<PagedResult<Order>> GetByUserIdPagedAsync(
    Guid userId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = orders,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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

    public async Task<Order?> GetOrderDetailsAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.AsNoTracking()
               .Include(o => o.Items).Include(o => o.Business).Include(o => o.Branch)
               .FirstOrDefaultAsync(o=> o.Id == orderId && o.UserId == userId, cancellationToken);
        return order;
    }

    public async Task<Order?> GetForCancellationAsync(Guid orderId,Guid userId,CancellationToken cancellationToken)
    {
        return await _context.Orders.Include(o=>o.Items)
             .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }
    public async Task <Order?> GetOrderForSatusUpdateAsync(Guid orderId,CancellationToken cancellationToken) {

        return await _context.Orders.Include(o => o.Business).Include(o=>o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetByBusinessIdPagedAsync(
    Guid businessId,
    OrderStatus? status,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking().Where(o => o.BusinessId == businessId);

        if (status.HasValue)
        {
            query.Where(o => o.Status == status.Value);
        }

        var totalCont = await query.CountAsync(cancellationToken);

        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Order>
        {
            Items = orders,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCont
        };
    }
}
