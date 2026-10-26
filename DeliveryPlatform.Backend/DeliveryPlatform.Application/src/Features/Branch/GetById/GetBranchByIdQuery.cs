using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.GetById;

public record GetBranchByIdQuery(
    Guid BusinessId,
    Guid BranchId,
    Guid UserId,
    bool IsAdmin
) : IRequest<BranchResponse?>;
