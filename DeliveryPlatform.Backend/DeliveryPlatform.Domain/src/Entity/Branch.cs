namespace DeliveryPlatform.Domain.src.Entity;

public class Branch
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public Business Business { get; set; } = null!;

    public ICollection<Inventory> Inventories { get; set; }
    = new List<Inventory>();
}