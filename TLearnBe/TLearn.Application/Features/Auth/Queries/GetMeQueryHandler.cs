using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Auth.Queries;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<UserDto>>

{
    private readonly UserManager<User> _userManager;

    private readonly TLearnDbContext _context;

    private readonly IConfiguration _configuration;

    public GetMeQueryHandler(
        UserManager<User> userManager,
        TLearnDbContext context,
        IConfiguration configuration)

    {
        _userManager = userManager;

        _context = context;

        _configuration = configuration;
    }

    public async Task<Result<UserDto>> Handle(
        GetMeQuery request,
        CancellationToken cancellationToken)

    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)

        {
            return Result<UserDto>.Failure("User not found");
        }

        var now = DateTime.UtcNow;

        var activeSubscription = await _context.Subscriptions
            .AsNoTracking()
            .Where(x =>
                x.UserId == user.Id &&
                x.IsActive &&
                x.EndDate > now)
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        var hasActiveSubscription = activeSubscription != null;

        var realSubscriptionType = hasActiveSubscription
            ? SubscriptionTypes.Vip
            : SubscriptionTypes.Free;

        if (!hasActiveSubscription &&
            string.Equals(user.SubscriptionType, SubscriptionTypes.Vip, StringComparison.OrdinalIgnoreCase))

        {
            user.SubscriptionType = SubscriptionTypes.Free;

            user.UpdatedAt = now;

            await _userManager.UpdateAsync(user);
        }

        var aiQuota = await GetAiUsageQuotaAsync(
            user.Id,
            realSubscriptionType,
            now,
            cancellationToken);

        return Result<UserDto>.Success(new UserDto

        {
            Id = user.Id,

            FullName = user.FullName ?? string.Empty,

            Email = user.Email ?? string.Empty,

            SubscriptionType = realSubscriptionType,

            EmailConfirmed = user.EmailConfirmed,

            CurrentSubscriptionId = activeSubscription?.Id,

            CurrentPlanType = activeSubscription?.PlanType,

            CurrentPlanName = activeSubscription?.PlanName,

            SubscriptionStartDate = activeSubscription?.StartDate,

            SubscriptionEndDate = activeSubscription?.EndDate,

            HasActiveSubscription = hasActiveSubscription,

            RemainingVipDays = activeSubscription == null
                ? null
                : Math.Max(
                    0,
                    (int)Math.Ceiling(
                        (activeSubscription.EndDate - now).TotalDays)),

            AiUsageLimit = aiQuota.Limit,

            AiUsedCount = aiQuota.UsedCount,

            AiRemainingCount = aiQuota.RemainingCount,

            AiUsageResetType = aiQuota.ResetType,

            AiUsageResetAt = aiQuota.ResetAt
        });
    }

    private async Task<AiUsageQuotaDto> GetAiUsageQuotaAsync(
        Guid userId,
        string subscriptionType,
        DateTime now,
        CancellationToken cancellationToken)

    {
        if (string.Equals(subscriptionType, SubscriptionTypes.Vip, StringComparison.OrdinalIgnoreCase))

        {
            var vipDailyLimit = _configuration.GetValue<int>(
                "AIUsageLimit:VipDailyLimit",
                10);

            var today = now.Date;

            var tomorrow = today.AddDays(1);

            var usedToday = await _context.AiUsageLogs
                .AsNoTracking()
                .CountAsync(x =>
                        x.UserId == userId &&
                        x.UsedAt >= today &&
                        x.UsedAt < tomorrow,
                    cancellationToken);

            return new AiUsageQuotaDto

            {
                Limit = vipDailyLimit,

                UsedCount = usedToday,

                RemainingCount = Math.Max(0, vipDailyLimit - usedToday),

                ResetType = "Daily",

                ResetAt = tomorrow
            };
        }

        var freeTotalLimit = _configuration.GetValue<int>(
            "AIUsageLimit:FreeTotalLimit",
            3);

        var usedTotal = await _context.AiUsageLogs
            .AsNoTracking()
            .CountAsync(x =>
                    x.UserId == userId,
                cancellationToken);

        return new AiUsageQuotaDto

        {
            Limit = freeTotalLimit,

            UsedCount = usedTotal,

            RemainingCount = Math.Max(0, freeTotalLimit - usedTotal),

            ResetType = "Never",

            ResetAt = null
        };
    }

    private class AiUsageQuotaDto

    {
        public int Limit { get; set; }

        public int UsedCount { get; set; }

        public int RemainingCount { get; set; }

        public string ResetType { get; set; } = string.Empty;

        public DateTime? ResetAt { get; set; }
    }
}