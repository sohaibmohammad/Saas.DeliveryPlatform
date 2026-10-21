using DeliveryPlatform.Application.src.Abstractions.Security;
using System.Security.Cryptography;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Security;

public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }

    public string HashRefreshToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

}   
