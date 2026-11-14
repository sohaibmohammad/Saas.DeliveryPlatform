using DeliveryPlatform.API.Hubs;
using DeliveryPlatform.Application.src.Abstractions.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryPlatform.API.Services.Notifications;

public class SignalROrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;
    public SignalROrderNotificationService(IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task NotifyOrderStatusChangedAsync(Guid userId, Guid orderId, string newStatus)
    {
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId = orderId,
                newStatus = newStatus,
                ChangedAt = DateTime.UtcNow
            });
    }
}
