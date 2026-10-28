using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.GetById;

public record GetProductByIdQuery(
    Guid BusinessId,
    Guid ProductId,
    Guid UserId,
    bool IsAdmin
) : IRequest<ProductByIdResponse?>;
