namespace DeliveryPlatform.Application.src.Features.Order.GetDetails;

public record OrderItemDetailsResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);