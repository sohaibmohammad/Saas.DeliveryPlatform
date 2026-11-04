using DeliveryPlatform.Application.src.Abstractions.Security;
using Microsoft.AspNetCore.Http;
  using System.Security.Claims;
 
namespace DeliveryPlatform.Infrastructure.src.Security;


public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? user?.FindFirstValue("sub");

            if (!Guid.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return id;
        }
    }
}