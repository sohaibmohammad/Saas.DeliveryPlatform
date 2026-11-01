namespace DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;

public record BranchInventoryResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity
);
