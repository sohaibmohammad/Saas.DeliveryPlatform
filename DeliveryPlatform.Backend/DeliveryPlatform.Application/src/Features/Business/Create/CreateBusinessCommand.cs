using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.Create;

public record CreateBusinessCommand(
    string Name,
    string? Description,
    BusinessType BusinessType,
    Guid UserId
) : IRequest<Guid>;
