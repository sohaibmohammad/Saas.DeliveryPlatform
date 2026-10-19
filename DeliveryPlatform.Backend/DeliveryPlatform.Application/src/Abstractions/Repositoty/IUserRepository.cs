using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IUserRepository:IGenericRepository<User>
{
    Task<bool> EmailExistsAsync(string email,CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
