using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStaus;

public record UpdaeOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<bool>;

