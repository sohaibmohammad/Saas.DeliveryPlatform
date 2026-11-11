using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStatus;

public record UpdateOrderStatusRequest(OrderStatus Status);
