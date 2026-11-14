
using DeliveryPlatform.Application.src.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace DeliveryPlatform.API.Hubs;

[Authorize]
public class OrderHub:Hub
{
    private readonly ICurrentUserService _currentUserService;

    public OrderHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;

        if (userId!=Guid.Empty)
        {
        await Groups.AddToGroupAsync(Context.ConnectionId,$"user-{userId}");
        }
      

        await base.OnConnectedAsync();
    }
}
