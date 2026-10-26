namespace DeliveryPlatform.Application.src.Features.Branch.Create;

public record CreateBranchRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude
);
