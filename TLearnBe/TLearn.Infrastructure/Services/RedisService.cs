using Microsoft.Extensions.Logging;
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
    private readonly IDatabase? _db;
    private readonly ILogger<RedisService> _logger;

    public RedisService(
        IConnectionMultiplexer? redis,
        ILogger<RedisService> logger)
    {
        _logger = logger;

        if (redis != null && redis.IsConnected)
        {
            _db = redis.GetDatabase();
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan expiry)
    {
        if (_db == null) return;

        try
        {
            await _db.StringSetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SetAsync lỗi, bỏ qua cache key {Key}", key);
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_db == null) return null;

        try
        {
            return await _db.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GetAsync lỗi, bỏ qua cache key {Key}", key);
            return null;
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (_db == null) return;

        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis RemoveAsync lỗi, bỏ qua cache key {Key}", key);
        }
    }

    public async Task SetRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry)
    {
        if (_db == null)
        {
            throw new InvalidOperationException("Redis chưa sẵn sàng để lưu refresh token.");
        }

        string key = $"refresh:{userId}:{refreshToken}";
        await _db.StringSetAsync(key, "valid", expiry - DateTime.UtcNow);
    }

    public async Task<bool> IsRefreshTokenValidAsync(Guid userId, string refreshToken)
    {
        if (_db == null)
        {
            throw new InvalidOperationException("Redis chưa sẵn sàng để kiểm tra refresh token.");
        }

        string key = $"refresh:{userId}:{refreshToken}";
        return await _db.KeyExistsAsync(key);
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken)
    {
        if (_db == null)
        {
            throw new InvalidOperationException("Redis chưa sẵn sàng để thu hồi refresh token.");
        }

        string key = $"refresh:{userId}:{refreshToken}";
        await _db.KeyDeleteAsync(key);
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId)
    {
        if (_db == null)
        {
            throw new InvalidOperationException("Redis chưa sẵn sàng để thu hồi refresh token.");
        }

        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        var keys = server.Keys(pattern: $"refresh:{userId}:*");

        foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}