using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class UserRefreshTokenRepository
    : GenericRepository<UserRefreshToken>,
      IUserRefreshTokenReposiory
{

    private readonly AppDbContext _context;

    public UserRefreshTokenRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRefreshToken>()
                  .Include(x => x.User) 
                  .FirstOrDefaultAsync(
                      x => x.TokenHash == tokenHash,
                      cancellationToken);
    }

  
}
