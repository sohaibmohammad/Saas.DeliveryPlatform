namespace DeliveryPlatform.Application.src.Features.Category.GetById;

public record CategoryByIdResponse(
    Guid Id,
    Guid BusinessId,
    string Name
);
