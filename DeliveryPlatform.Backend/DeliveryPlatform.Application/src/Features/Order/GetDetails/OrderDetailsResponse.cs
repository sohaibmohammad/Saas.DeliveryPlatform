using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetDetails;

public record OrderDetailsResponse(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    string BusinessName,
    string BranchName,
    string DeliveryAddress,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemDetailsResponse> Items
);
