using DeliveryPlatform.Application.src.Abstractions.Security;
using StackExchange.Redis;


namespace DeliveryPlatform.Infrastructure.src.Security;

public class RedisTokenRevocationService : ITokenRevocationService
{
    private readonly IDatabase _database;

    public RedisTokenRevocationService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task RevokeAsync(string jti, DateTime expiresAt)
    {
        var ttl = expiresAt.ToUniversalTime() - DateTime.UtcNow;

        if (ttl <= TimeSpan.Zero)
            return;

        await _database.StringSetAsync(
            $"revoked:access:{jti}",
            "1",
            ttl);
    }

    public async Task<bool> IsRevokedAsync(string jti)
    {
        return await _database.KeyExistsAsync(
            $"revoked:access:{jti}");
    }
}