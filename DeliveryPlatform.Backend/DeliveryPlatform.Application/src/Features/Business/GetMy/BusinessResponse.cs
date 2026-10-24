using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Features.Business.GetMy;

public record BusinessResponse(
    Guid Id,
    string Name,
    string? Description,
    BusinessType BusinessType,
    bool IsActive,
    DateTime CreatedAt
);
