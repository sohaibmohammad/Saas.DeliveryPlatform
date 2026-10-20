using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;

public class LoginWithGoogleHandler
    : IRequestHandler<LoginWithGoogleCommand, string>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginWithGoogleHandler(
        IGoogleAuthService googleAuthService,
        IExternalLoginRepository externalLoginRepository,
        IJwtTokenService jwtTokenService)
    {
        _googleAuthService = googleAuthService;
        _externalLoginRepository = externalLoginRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        var googleUser =
            await _googleAuthService.ValidateIdTokenAsync(
                request.GoogleIdToken,
                cancellationToken);

        if (googleUser is null || !googleUser.EmailVerified)
        {
            throw new UnauthorizedAccessException(
                "Invalid Google credentials.");
        }

        var user = await _externalLoginRepository
            .GetUserByProviderAsync(
                "Google",
                googleUser.Subject,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "No account is linked to this Google account.");
        }

        return _jwtTokenService.GenerateToken(user);
    }
}