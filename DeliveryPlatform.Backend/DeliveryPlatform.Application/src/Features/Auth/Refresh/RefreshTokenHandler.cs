using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Refresh;

class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IUserRefreshTokenReposiory refreshTokenRepository,
        IRefreshTokenService refreshTokenService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashRefreshToken(request.RefreshToken);

        var oldToken = await _refreshTokenRepository
            .GetByTokenHashAsync(tokenHash, cancellationToken);

        if (oldToken is null ||
            oldToken.RevokedAt is not null ||
            oldToken.ExpiryDate <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = oldToken.User;

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token user.");
        }

        var accessToken = _jwtTokenService.GenerateToken(user);

        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
        var newTokenHash = _refreshTokenService.HashRefreshToken(newRefreshToken);

        var newToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = oldToken.UserId,
            TokenHash = newTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        var revoked = await _refreshTokenRepository.TryRevokeAsync(
     oldToken.Id,
     newToken.Id,
     cancellationToken);

        if (!revoked)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has already been used.");
        }

        await _refreshTokenRepository.AddAsync(newToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(
            accessToken,
            newRefreshToken);
    }
}
