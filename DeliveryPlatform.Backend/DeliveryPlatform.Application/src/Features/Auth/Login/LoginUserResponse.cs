namespace DeliveryPlatform.Application.src.Features.Auth.Login;

public record LoginUserResponse(
    string AccessToken,
    string RefreshToken
);