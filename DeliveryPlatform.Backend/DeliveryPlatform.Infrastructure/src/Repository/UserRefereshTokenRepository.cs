using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
    public async Task<bool> TryRevokeAsync(
    Guid tokenId,
    Guid replacementTokenId,
    CancellationToken cancellationToken)
    {
        var affectedRows = await _context.UserRefreshTokens
            .Where(x =>
                x.Id == tokenId &&
                x.RevokedAt == null &&
                x.RevokedAt == null &&
                x.ExpiryDate > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RevokedAt, DateTime.UtcNow)
                .SetProperty(x => x.ReplacedByTokenId, replacementTokenId),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task<int> RevokeAllByUserIdAsync(Guid userId,CancellationToken cancellationToken) {

        var now = DateTime.UtcNow;

        return await _context.UserRefreshTokens
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAt == null &&
                x.ExpiryDate > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RevokedAt, now),
                cancellationToken);
    }
}
