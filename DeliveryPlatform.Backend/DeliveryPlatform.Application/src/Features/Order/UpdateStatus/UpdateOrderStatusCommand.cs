using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<bool>;
