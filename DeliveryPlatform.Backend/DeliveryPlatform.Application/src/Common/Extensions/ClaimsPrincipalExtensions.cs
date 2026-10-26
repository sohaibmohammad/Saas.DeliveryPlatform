using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace DeliveryPlatform.API.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var id) ? id : null;
    }
}