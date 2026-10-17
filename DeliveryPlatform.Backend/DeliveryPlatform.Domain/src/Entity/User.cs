using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Domain.src.Entity;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
}
