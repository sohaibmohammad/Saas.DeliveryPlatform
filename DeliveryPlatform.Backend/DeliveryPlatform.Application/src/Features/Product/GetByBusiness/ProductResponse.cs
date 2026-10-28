namespace DeliveryPlatform.Application.src.Features.Product.GetByBusiness;

public record ProductResponse(
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
