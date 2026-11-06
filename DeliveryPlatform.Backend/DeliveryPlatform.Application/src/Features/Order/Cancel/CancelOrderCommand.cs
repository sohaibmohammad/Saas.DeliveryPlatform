using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.Cancel;

public record CancelOrderCommand(Guid OrderId):IRequest<bool>;
