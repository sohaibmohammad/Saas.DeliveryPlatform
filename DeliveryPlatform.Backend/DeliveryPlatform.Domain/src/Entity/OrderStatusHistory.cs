namespace DeliveryPlatform.Domain.src.Entity;

public class OrderStatusHistory
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public OrderStatus? PreviousStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public Guid ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Note { get; set; }

    public Order Order { get; set; } = null!;

    public User User { get; set; } = null!;
}