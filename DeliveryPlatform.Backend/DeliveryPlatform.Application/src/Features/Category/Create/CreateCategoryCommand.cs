using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.Create;

public record CreateCategoryCommand(
    Guid BusinessId,
    string Name,
    Guid UserId
) : IRequest<Guid>;
