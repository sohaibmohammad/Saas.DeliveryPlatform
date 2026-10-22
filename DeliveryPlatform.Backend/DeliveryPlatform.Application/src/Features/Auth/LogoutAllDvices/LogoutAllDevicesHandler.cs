using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.LogoutAllDvices;

public class LogoutAllDevicesHandler : IRequestHandler<LogoutAllDevicesCommand>
{
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;

    public LogoutAllDevicesHandler(
        IUserRefreshTokenReposiory refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task Handle(
        LogoutAllDevicesCommand request,
        CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllByUserIdAsync(
            request.UserId,
            cancellationToken);
    }
}
