namespace DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;

public record BranchResponse(
    Guid Id,
    Guid BusinessId,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    bool IsActive
);
