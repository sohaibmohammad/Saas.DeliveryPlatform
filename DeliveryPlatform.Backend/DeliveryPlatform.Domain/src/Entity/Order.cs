namespace DeliveryPlatform.Domain.src.Entity;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BusinessId { get; set; }

    public Guid BranchId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal Subtotal { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal Total { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;

    public Business Business { get; set; } = null!;

    public Branch Branch { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; }
    = new List<OrderStatusHistory>();
}
