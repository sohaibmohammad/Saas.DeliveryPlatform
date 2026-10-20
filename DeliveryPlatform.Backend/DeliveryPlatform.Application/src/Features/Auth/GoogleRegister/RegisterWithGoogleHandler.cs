using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.GoogleRegister;

public class RegisterWithGoogleHandler : IRequestHandler<RegisterWithGoogleCommand, string>
{

    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterWithGoogleHandler(
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        IExternalLoginRepository externalLoginRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _externalLoginRepository = externalLoginRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> Handle(
        RegisterWithGoogleCommand request,
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

        var existingGoogleUser =
            await _externalLoginRepository.GetUserByProviderAsync(
                "Google",
                googleUser.Subject,
                cancellationToken);

        if (existingGoogleUser is not null)
        {
            throw new InvalidOperationException(
                "This Google account is already registered. Please log in.");
        }

        var existingEmail = await _userRepository.GetByEmailAsync(
            googleUser.Email,
            cancellationToken);

        if (existingEmail is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = googleUser.Email,
            Email = googleUser.Email,
            PasswordHash = null,
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

         var externalLogin = new ExternalLogins
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = "Google",
            ProviderUserId = googleUser.Subject,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _externalLoginRepository.AddAsync(
                externalLogin,
            cancellationToken);

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return _jwtTokenService.GenerateToken(user);
    }
}
