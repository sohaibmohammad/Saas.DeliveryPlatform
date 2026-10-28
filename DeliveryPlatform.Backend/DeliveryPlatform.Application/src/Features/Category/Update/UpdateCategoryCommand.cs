using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.Update;

public record UpdateCategoryCommand(
    Guid BusinessId,
    Guid CategoryId,
    string Name,
    Guid UserId
) : IRequest<Guid>;
