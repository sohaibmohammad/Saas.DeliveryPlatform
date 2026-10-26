using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Features.Business.Update;

public record UpdateBusinessRequest(
    string Name,
    string? Description,
    BusinessType BusinessType,
    bool IsActive
);
