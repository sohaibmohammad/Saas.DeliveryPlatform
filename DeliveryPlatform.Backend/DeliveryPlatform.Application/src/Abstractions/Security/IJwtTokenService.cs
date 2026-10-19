using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}