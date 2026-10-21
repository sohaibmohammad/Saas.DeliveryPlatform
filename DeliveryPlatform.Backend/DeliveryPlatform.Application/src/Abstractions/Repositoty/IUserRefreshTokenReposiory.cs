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

      
    }
}
