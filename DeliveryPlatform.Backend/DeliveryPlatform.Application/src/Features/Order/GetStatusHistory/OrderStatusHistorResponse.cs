using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetStatusHistory;

public class OrderStatusHistorResponse
{
    public OrderStatus? PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
