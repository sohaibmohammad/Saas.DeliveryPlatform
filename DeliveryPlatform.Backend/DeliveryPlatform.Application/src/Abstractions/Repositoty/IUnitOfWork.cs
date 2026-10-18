using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
