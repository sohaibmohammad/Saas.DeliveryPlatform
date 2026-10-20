using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Security;

public record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified
);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken);
}
