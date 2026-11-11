using System.Security.Claims;
using DeliveryPlatform.API.Common.Extensions;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnUserId_FromNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_ShouldReturnUserId_FromSubClaim()
    {
        var userId = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim("sub", userId.ToString()));

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_ShouldPreferNameIdentifierClaim()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.NameIdentifier, expected.ToString()),
            new Claim("sub", Guid.NewGuid().ToString()));

        var result = principal.GetUserId();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenClaimIsMissing()
    {
        var principal = CreatePrincipal();

        var result = principal.GetUserId();

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenClaimIsNotGuid()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));

        var result = principal.GetUserId();

        Assert.Null(result);
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test"));
}
