namespace DeliveryPlatform.Application.src.Features.Category.GetByBusiness;

public record CategoryResponse(
    Guid Id,
    Guid BusinessId,
    string Name
);
