using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class ExternalLoginRepository : GenericRepository<ExternalLogins>,IExternalLoginRepository
{


    private readonly AppDbContext _context;

    public ExternalLoginRepository(AppDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByProviderAsync(string provider, string providerUserId, CancellationToken cancellationToken)
    {
        return await _context.ExternalLogins
          .Where(x =>
              x.Provider == provider &&
              x.ProviderUserId == providerUserId)
          .Select(x => x.User)
          .FirstOrDefaultAsync(cancellationToken);

     }
}
