using Microsoft.Extensions.Logging;
using TLearn.Domain.Constants;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Services.Subscriptions;

namespace TLearn.Infrastructure.Services.Payments;

public class PaymentStatusService : IPaymentStatusService

{
    private readonly ISubscriptionService _subscriptionService;

    private readonly ILogger<PaymentStatusService> _logger;

    public PaymentStatusService(
        ISubscriptionService subscriptionService,
        ILogger<PaymentStatusService> logger)

    {
        _subscriptionService = subscriptionService;

        _logger = logger;
    }

    public async Task MarkPaidAndActivateSubscriptionAsync(
        Payment payment,
        string? rawWebhookJson,
        string? payOSStatus,
        CancellationToken cancellationToken)
    {
        if (payment.Status == PaymentStatuses.Paid)
        {
            payment.RawWebhookJson = rawWebhookJson ?? payment.RawWebhookJson;
            payment.LastPayOSStatus = payOSStatus ?? payment.LastPayOSStatus;
            payment.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Payment {OrderCode} đã PAID trước đó, bỏ qua activate subscription.",
                payment.PayOSOrderCode);

            return;
        }

        payment.Status = PaymentStatuses.Paid;

        payment.PaidAt ??= DateTime.UtcNow;

        payment.RawWebhookJson = rawWebhookJson ?? payment.RawWebhookJson;

        payment.LastPayOSStatus = string.IsNullOrWhiteSpace(payOSStatus)

            ? PaymentStatuses.Paid

            : payOSStatus;

        payment.UpdatedAt = DateTime.UtcNow;

        await _subscriptionService.ActivateOrExtendVipAsync(
            payment,
            cancellationToken);
    }

    public Task MarkCancelledAsync(
        Payment payment,
        CancellationToken cancellationToken)

    {
        // Nếu đã PAID thì không cho cancel ghi đè.

        if (payment.Status == PaymentStatuses.Paid)

        {
            return Task.CompletedTask;
        }

        payment.Status = PaymentStatuses.Cancelled;

        payment.CancelledAt ??= DateTime.UtcNow;

        payment.UpdatedAt = DateTime.UtcNow;

        payment.LastPayOSStatus = PaymentStatuses.Cancelled;

        return Task.CompletedTask;
    }

    public Task MarkExpiredAsync(
        Payment payment,
        CancellationToken cancellationToken)

    {
        // Nếu đã PAID thì không cho expired ghi đè.

        if (payment.Status == PaymentStatuses.Paid)

        {
            return Task.CompletedTask;
        }

        payment.Status = PaymentStatuses.Expired;

        payment.ExpiredAt ??= DateTime.UtcNow;

        payment.UpdatedAt = DateTime.UtcNow;

        payment.LastPayOSStatus = PaymentStatuses.Expired;

        return Task.CompletedTask;
    }
}