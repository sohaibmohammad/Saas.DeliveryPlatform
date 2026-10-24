using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.LogoutAllDvices;

public class LogoutAllDevicesHandler : IRequestHandler<LogoutAllDevicesCommand>
{
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutAllDevicesHandler(
        IUserRefreshTokenReposiory refreshTokenRepository,IUserRepository userRepository,IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        LogoutAllDevicesCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var user= await _userRepository.GetByIdAsync(request.UserId, ct);
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                user.TokenVersion++;

                await _refreshTokenRepository.RevokeAllByUserIdAsync(
                    request.UserId,
                    ct);
            }
            );
    }
}
