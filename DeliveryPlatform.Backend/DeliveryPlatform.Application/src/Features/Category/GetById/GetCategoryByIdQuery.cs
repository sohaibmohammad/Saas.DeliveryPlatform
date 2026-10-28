using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.GetById;

public record GetCategoryByIdQuery(
    Guid BusinessId,
    Guid CategoryId,
    Guid UserId,
    bool IsAdmin
) : IRequest<CategoryByIdResponse?>;
