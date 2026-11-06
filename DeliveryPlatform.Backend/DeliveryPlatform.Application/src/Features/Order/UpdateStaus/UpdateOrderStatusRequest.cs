using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStaus;

public record UpdateOrderStatusRequest(OrderStatus Statuss);
