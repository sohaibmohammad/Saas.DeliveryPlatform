using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.Update;

public record UpdateBranchCommand(
    Guid BusinessId,
    Guid BranchId,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    bool IsActive,
    Guid UserId
) : IRequest<Guid>;
