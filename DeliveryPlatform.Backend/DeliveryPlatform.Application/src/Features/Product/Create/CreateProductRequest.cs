namespace DeliveryPlatform.Application.src.Features.Product.Create;

public record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable = true
);
