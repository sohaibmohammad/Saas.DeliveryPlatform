using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.Update;

public record UpdateProductCommand(
    Guid BusinessId,
    Guid ProductId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    Guid UserId
) : IRequest<Guid>;
