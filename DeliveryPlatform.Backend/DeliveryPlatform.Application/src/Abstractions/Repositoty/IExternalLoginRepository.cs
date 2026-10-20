using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IExternalLoginRepository : IGenericRepository<ExternalLogins>
{
    Task<User?> GetUserByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken);
}
