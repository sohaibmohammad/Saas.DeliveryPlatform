using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
