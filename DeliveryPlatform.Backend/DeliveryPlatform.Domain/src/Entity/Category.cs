namespace DeliveryPlatform.Domain.src.Entity;

public class Category
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Name { get; set; } = null!;

    // Navigation
    public Business Business { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}