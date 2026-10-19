using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(
    email,
    cancellationToken))
        {
            throw new InvalidOperationException(
                "Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = email,
            Role = UserRole.Customer
        };

        user.PasswordHash = _passwordHashService.HashPassword(
         user,
         request.Password);

        await _userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
