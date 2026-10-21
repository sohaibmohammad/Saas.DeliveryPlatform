namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    string HashRefreshToken(string token);

}