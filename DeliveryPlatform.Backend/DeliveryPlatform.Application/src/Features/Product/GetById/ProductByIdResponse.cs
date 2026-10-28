namespace DeliveryPlatform.Application.src.Features.Product.GetById;

public record ProductByIdResponse(
    Guid Id,
    Guid BusinessId,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
