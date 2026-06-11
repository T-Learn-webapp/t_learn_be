using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Services.AdminNotifications;

public interface IAdminNotificationService

{
    Task NotifyPaymentPaidAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}