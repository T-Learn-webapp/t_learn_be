using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Services.Subscriptions;

public interface ISubscriptionService
{
    Task ActivateOrExtendVipAsync(
        Payment payment,
        CancellationToken cancellationToken);
}