using DeliveryPlatform.API.Options;
using DeliveryPlatform.Application.src.Abstractions.Security;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DeliveryPlatform.Infrastructure.src.Security;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthOptions _options;

    public GoogleAuthService(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo?> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _options.ClientId }
                });

            cancellationToken.ThrowIfCancellationRequested();

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}