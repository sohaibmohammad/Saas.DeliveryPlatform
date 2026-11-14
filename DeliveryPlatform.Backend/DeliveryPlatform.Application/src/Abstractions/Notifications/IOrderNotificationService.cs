using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Notifications;

public interface IOrderNotificationService  
{
    Task NotifyOrderStatusChangedAsync(
     Guid userId,
     Guid orderId,
     string newStatus);
}
