namespace DeliveryPlatform.Domain.src.Entity;

public class Product
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Business Business { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public ICollection<Inventory> Inventories { get; set; }
        = new List<Inventory>();
}