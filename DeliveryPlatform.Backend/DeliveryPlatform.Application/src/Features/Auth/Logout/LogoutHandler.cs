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
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        IUserRefreshTokenReposiory refreshTokenRepository,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var tokenHash = _refreshTokenService
            .HashRefreshToken(request.RefreshToken);
        var token = await _refreshTokenRepository
            .GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token is null)
            return;

        if (token.RevokedAt is not null)
            return;

        token.RevokedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
