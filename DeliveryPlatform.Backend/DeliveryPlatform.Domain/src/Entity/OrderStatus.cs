namespace DeliveryPlatform.Domain.src.Entity;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    ReadyForPickup,
    OutForDelivery,
    Delivered,
    Cancelled
}