using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IGenericRepository<TEntity> where TEntity : class
{
    public  Task AddAsync(
       TEntity entity,
       CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
