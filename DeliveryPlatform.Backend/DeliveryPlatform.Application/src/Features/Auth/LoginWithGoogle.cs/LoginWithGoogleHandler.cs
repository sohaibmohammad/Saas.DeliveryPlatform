using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.Login;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;

public class LoginWithGoogleHandler
    : IRequestHandler<LoginWithGoogleCommand, LoginUserResponse>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;

    private readonly IUnitOfWork _unitOfWork;
    public LoginWithGoogleHandler(
        IGoogleAuthService googleAuthService,
        IExternalLoginRepository externalLoginRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IUserRefreshTokenReposiory userRefreshTokenReposiory,
        IUnitOfWork unitOfWork
        )
    {
        _googleAuthService = googleAuthService;
        _externalLoginRepository = externalLoginRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = userRefreshTokenReposiory;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginUserResponse> Handle(
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
        var accessToken = _jwtTokenService.GenerateToken(user);
        var refreshToken =
        _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash =
            _refreshTokenService.HashRefreshToken(refreshToken);


        var tokenEntity = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            RevokedAt = null,
            ReplacedByTokenId = null
        };
        await _refreshTokenRepository.AddAsync(tokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse(
            accessToken,
            refreshToken);


    }
}