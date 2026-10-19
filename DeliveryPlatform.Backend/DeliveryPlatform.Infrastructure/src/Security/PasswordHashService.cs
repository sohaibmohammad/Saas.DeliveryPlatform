using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Security;

public class PasswordHashService : IPasswordHashService
{
    private readonly IPasswordHasher<User> _passwordHasher;
    public PasswordHashService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(User user, string password)
    {
      return  _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user,  string passwordHash, string password)
    {
        var result= _passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}
