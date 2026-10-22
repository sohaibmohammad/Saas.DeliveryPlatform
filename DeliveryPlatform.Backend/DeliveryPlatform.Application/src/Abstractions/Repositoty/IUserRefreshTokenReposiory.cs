using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty
{
    public interface IUserRefreshTokenReposiory : IGenericRepository<UserRefreshToken>
    {

        Task<UserRefreshToken?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default);
        Task<int> RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> TryRevokeAsync(Guid tokenId, Guid replacementTokenId, CancellationToken cancellationToken);
    }
}
