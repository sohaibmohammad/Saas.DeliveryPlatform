using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.Update;

public record UpdateBusinessCommand(
    Guid BusinessId,
    string Name,
    string? Description,
    BusinessType BusinessType,
    bool IsActive,
    Guid UserId
) : IRequest<Guid>;
