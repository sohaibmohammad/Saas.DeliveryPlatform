namespace DeliveryPlatform.Application.src.Features.Branch.Update;

public record UpdateBranchRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    bool IsActive
);
