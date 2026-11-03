using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(AppDbContext context) : base(context)
    {
    }
}
