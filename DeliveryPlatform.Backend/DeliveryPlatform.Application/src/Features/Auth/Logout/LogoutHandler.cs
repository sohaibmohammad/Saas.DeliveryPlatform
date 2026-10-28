using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenRevocationService _tokenRevocationService;

    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(

        IUserRefreshTokenReposiory refreshTokenRepository,
        IRefreshTokenService refreshTokenService,
        ITokenRevocationService tokenRevocationService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _tokenRevocationService = tokenRevocationService;
    }
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = _refreshTokenService
                .HashRefreshToken(request.RefreshToken);

            var token = await _refreshTokenRepository
                .GetByTokenHashAsync(tokenHash, cancellationToken);

            if (token is not null &&
                token.UserId == request.UserId &&
                token.RevokedAt is null)
            {
                token.RevokedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        // Revoke the current access token regardless of refresh token status.
        await _tokenRevocationService.RevokeAsync(
            request.Jti,
            request.AccessTokenExpiresAt);
    }
}

