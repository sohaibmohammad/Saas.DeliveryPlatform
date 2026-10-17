namespace DeliveryPlatform.Domain.src.Entity;

public class Business
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public BusinessType BusinessType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Owner { get; set; } = null!;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}