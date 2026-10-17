namespace DeliveryPlatform.Domain.src.Entity;

public class Inventory
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public int ReservedQuantity { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public Product Product { get; set; } = null!;
}