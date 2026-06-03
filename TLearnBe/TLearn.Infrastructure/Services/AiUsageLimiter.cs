using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Infrastructure.Services;

public interface IAiUsageLimiter
{
    Task<AiUsageCheckResult> CheckAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task RecordAsync(
        Guid userId,
        string feature,
        CancellationToken cancellationToken);
}

public class AiUsageLimiter : IAiUsageLimiter

{
    private readonly TLearnDbContext _context;

    private readonly IConfiguration _configuration;

    public AiUsageLimiter(
        TLearnDbContext context,
        IConfiguration configuration)

    {
        _context = context;

        _configuration = configuration;
    }

    public async Task<AiUsageCheckResult> CheckAsync(
        Guid userId,
        CancellationToken cancellationToken)

    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)

        {
            return new AiUsageCheckResult

            {
                IsAllowed = false,

                ErrorMessage = "Người dùng không tồn tại."
            };
        }

        if (!user.IsActive)

        {
            return new AiUsageCheckResult

            {
                IsAllowed = false,

                ErrorMessage = "Tài khoản đã bị vô hiệu hoá."
            };
        }

        var subscriptionType = user.SubscriptionType;

        if (string.Equals(subscriptionType, SubscriptionTypes.Vip, StringComparison.OrdinalIgnoreCase))

        {
            return await CheckVipAsync(userId, cancellationToken);
        }

        return await CheckFreeAsync(userId, cancellationToken);
    }

    public async Task RecordAsync(
        Guid userId,
        string feature,
        CancellationToken cancellationToken)

    {
        var log = new AiUsageLog

        {
            UserId = userId,

            Feature = feature,

            UsedAt = DateTime.UtcNow
        };

        _context.AiUsageLogs.Add(log);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<AiUsageCheckResult> CheckFreeAsync(
        Guid userId,
        CancellationToken cancellationToken)

    {
        var freeTotalLimit = _configuration.GetValue<int>(
            "AIUsageLimit:FreeTotalLimit",
            3);

        var usedCount = await _context.AiUsageLogs
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);

        if (usedCount >= freeTotalLimit)

        {
            return new AiUsageCheckResult

            {
                IsAllowed = false,

                UsedCount = usedCount,

                Limit = freeTotalLimit,

                SubscriptionType = SubscriptionTypes.Free,

                ErrorMessage =
                    $"Tài khoản Free chỉ được dùng AI tối đa {freeTotalLimit} lần. Vui lòng nâng cấp Vip để tiếp tục sử dụng."
            };
        }

        return new AiUsageCheckResult

        {
            IsAllowed = true,

            UsedCount = usedCount,

            Limit = freeTotalLimit,

            SubscriptionType = SubscriptionTypes.Free
        };
    }

    private async Task<AiUsageCheckResult> CheckVipAsync(
        Guid userId,
        CancellationToken cancellationToken)

    {
        var vipDailyLimit = _configuration.GetValue<int>(
            "AIUsageLimit:VipDailyLimit",
            10);

        var today = DateTime.UtcNow.Date;

        var tomorrow = today.AddDays(1);

        var usedToday = await _context.AiUsageLogs
            .AsNoTracking()
            .CountAsync(x =>
                    x.UserId == userId &&
                    x.UsedAt >= today &&
                    x.UsedAt < tomorrow,
                cancellationToken);

        if (usedToday >= vipDailyLimit)

        {
            return new AiUsageCheckResult

            {
                IsAllowed = false,

                UsedCount = usedToday,

                Limit = vipDailyLimit,

                SubscriptionType = SubscriptionTypes.Vip,

                ErrorMessage =
                    $"Tài khoản Vip chỉ được dùng AI tối đa {vipDailyLimit} lần mỗi ngày. Vui lòng thử lại vào ngày mai."
            };
        }

        return new AiUsageCheckResult

        {
            IsAllowed = true,

            UsedCount = usedToday,

            Limit = vipDailyLimit,

            SubscriptionType = SubscriptionTypes.Vip
        };
    }
}

public class AiUsageCheckResult
{
    public bool IsAllowed { get; set; }

    public string? ErrorMessage { get; set; }

    public int UsedCount { get; set; }

    public int Limit { get; set; }

    public string SubscriptionType { get; set; } = string.Empty;
}