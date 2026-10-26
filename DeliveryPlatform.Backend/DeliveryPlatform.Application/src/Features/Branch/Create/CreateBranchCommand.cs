using MediatR;

namespace DeliveryPlatform.Application.src.Features.Branch.Create;

public record CreateBranchCommand(
    Guid BusinessId,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    Guid UserId
) : IRequest<Guid>;
