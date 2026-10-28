using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.Create;

public record CreateProductCommand(
    Guid BusinessId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    Guid UserId
) : IRequest<Guid>;
