using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryPlatform.Domain.src.Entity;

[Table("UserRefreshToken")]

public class UserRefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public User User { get; set; } = null!;
}