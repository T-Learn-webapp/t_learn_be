using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using SubscriptionTypes = TLearn.Domain.Constants.SubscriptionTypes;

namespace TLearn.Infrastructure.Services.Subscriptions;

public class SubscriptionService : ISubscriptionService

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        TLearnDbContext context,
        ILogger<SubscriptionService> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task ActivateOrExtendVipAsync(
        Payment payment,
        CancellationToken cancellationToken)

    {
        if (payment.UserId == Guid.Empty)

        {
            throw new InvalidOperationException("Payment không có UserId hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(payment.PayOSOrderCode))

        {
            throw new InvalidOperationException("Payment không có PayOSOrderCode.");
        }

        if (payment.DurationDays <= 0)

        {
            throw new InvalidOperationException("Payment không có DurationDays hợp lệ.");
        }

        // Chống webhook gửi lại nhiều lần gây tạo/gia hạn trùng subscription

        var existedSubscription = await _context.Subscriptions
            .AnyAsync(
                x => x.PayOSOrderCode == payment.PayOSOrderCode,
                cancellationToken);

        if (existedSubscription)

        {
            _logger.LogInformation(
                "Subscription đã tồn tại cho PayOSOrderCode {OrderCode}, bỏ qua gia hạn.",
                payment.PayOSOrderCode);

            return;
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.Id == payment.UserId,
                cancellationToken);

        if (user == null)

        {
            throw new InvalidOperationException("Không tìm thấy người dùng của payment.");
        }

        var now = DateTime.UtcNow;

        var activeSubscription = await _context.Subscriptions
            .Where(x =>
                x.UserId == payment.UserId &&
                x.IsActive &&
                x.EndDate > now)
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSubscription == null)

        {
            var subscription = new Subscription

            {
                UserId = payment.UserId,

                PlanType = payment.PlanType,

                PlanName = payment.PlanName,

                StartDate = now,

                EndDate = now.AddDays(payment.DurationDays),

                IsActive = true,

                PayOSOrderCode = payment.PayOSOrderCode,

                CreatedAt = now
            };

            _context.Subscriptions.Add(subscription);
        }

        else

        {
            activeSubscription.EndDate =
                activeSubscription.EndDate.AddDays(payment.DurationDays);

            activeSubscription.PlanType = payment.PlanType;

            activeSubscription.PlanName = payment.PlanName;

            activeSubscription.PayOSOrderCode = payment.PayOSOrderCode;

            
        }

        user.SubscriptionType = SubscriptionTypes.Vip;

        user.UpdatedAt = now;
    }
}