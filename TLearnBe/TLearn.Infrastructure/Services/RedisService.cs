using StackExchange.Redis;

namespace TLearn.Infrastructure.Services;

public interface IRedisService
{
    Task SetRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry);
    Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken);
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken);
    Task RevokeAllRefreshTokensAsync(Guid userId);
    
    Task SetAsync(string key, string value, TimeSpan expiry);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}

public class RedisService : IRedisService
{
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry)
    {
        string key = $"refresh:{userId}:{refreshToken}";
        await _db.StringSetAsync(key, "valid", expiry - DateTime.UtcNow);
    }

    public async Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken)
    {
        string key = $"refresh:{userId}:{refreshToken}";
        return await _db.KeyExistsAsync(key);
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken)
    {
        string key = $"refresh:{userId}:{refreshToken}";
        await _db.KeyDeleteAsync(key);
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId)
    {
        // Xóa tất cả refresh token của user (dùng pattern scan nếu cần)
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        var keys = server.Keys(pattern: $"refresh:{userId}:*");
        foreach (var key in keys)
            await _db.KeyDeleteAsync(key);
    }
    
    public async Task SetAsync(string key, string value, TimeSpan expiry)
    {
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}