namespace DeliveryPlatform.Application.src.Abstractions.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }
}