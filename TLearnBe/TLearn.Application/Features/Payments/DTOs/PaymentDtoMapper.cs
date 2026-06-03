using TLearn.Domain.Constants;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.Payments.DTOs;

public static class PaymentDtoMapper

{
    public static PaymentStatusDto ToStatusDto(Payment payment)

    {
        return new PaymentStatusDto

        {
            PaymentId = payment.Id,

            OrderCode = payment.PayOSOrderCode,

            Status = payment.Status,

            LastPayOSStatus = payment.LastPayOSStatus,

            Amount = payment.Amount,

            Currency = payment.Currency,

            PlanType = payment.PlanType,

            PlanName = payment.PlanName,

            ExpiresAt = payment.ExpiresAt,

            PaidAt = payment.PaidAt,

            CancelledAt = payment.CancelledAt,

            ExpiredAt = payment.ExpiredAt,

            IsPaid = payment.Status == PaymentStatuses.Paid,

            IsExpired = payment.Status == PaymentStatuses.Expired,

            IsCancelled = payment.Status == PaymentStatuses.Cancelled
        };
    }
}