using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions;

public interface IGenericRepository<TEntity> where TEntity : class
{
    public  Task AddAsync(
       TEntity entity,
       CancellationToken cancellationToken);
      
   }
