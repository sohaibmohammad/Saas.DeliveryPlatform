namespace DeliveryPlatform.Application.src.Features.Product.Update;

public record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable
);
