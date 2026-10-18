using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface IPasswordHashService
{
    string HashPassword(User user,string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}
