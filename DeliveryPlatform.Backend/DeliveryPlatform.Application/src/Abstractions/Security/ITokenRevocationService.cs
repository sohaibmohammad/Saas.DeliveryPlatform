namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface ITokenRevocationService
{
    Task RevokeAsync(
        string jti,
        DateTime expiresAt);

    Task<bool> IsRevokedAsync(string jti);
}