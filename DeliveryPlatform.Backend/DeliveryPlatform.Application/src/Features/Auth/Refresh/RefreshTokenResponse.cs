namespace DeliveryPlatform.Application.src.Features.Auth.Refresh;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);