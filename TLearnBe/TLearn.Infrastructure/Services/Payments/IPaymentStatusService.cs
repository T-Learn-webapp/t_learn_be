using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Services.Payments;

public interface IPaymentStatusService

{
    Task MarkPaidAndActivateSubscriptionAsync(
        Payment payment,
        string? rawWebhookJson,
        string? payOSStatus,
        CancellationToken cancellationToken);

    Task MarkCancelledAsync(
        Payment payment,
        CancellationToken cancellationToken);

    Task MarkExpiredAsync(
        Payment payment,
        CancellationToken cancellationToken);
}