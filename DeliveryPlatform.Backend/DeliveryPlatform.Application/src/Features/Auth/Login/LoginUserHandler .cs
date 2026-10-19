using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Login;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;

    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
    }
    public async Task<Guid> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var isPasswordValid = _passwordHashService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return user.Id;
    }

}
