using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;
using TLearn.Domain.Constants;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;
using TLearn.Infrastructure.Services.Payments;
using TLearn.Infrastructure.Services.PayOs;

namespace TLearn.Application.Features.Payments.Commands.SyncPaymentStatus;

public class SyncPaymentStatusCommandHandler
    : IRequestHandler<SyncPaymentStatusCommand, Result<PaymentStatusDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IPayOSService _payOSService;

    private readonly IPaymentStatusService _paymentStatusService;

    private readonly ILogger<SyncPaymentStatusCommandHandler> _logger;

    public SyncPaymentStatusCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IPayOSService payOSService,
        IPaymentStatusService paymentStatusService,
        ILogger<SyncPaymentStatusCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _payOSService = payOSService;

        _paymentStatusService = paymentStatusService;

        _logger = logger;
    }

    public async Task<Result<PaymentStatusDto>> Handle(
        SyncPaymentStatusCommand request,
        CancellationToken cancellationToken)

    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try

        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<PaymentStatusDto>.Failure("Chưa đăng nhập.");
            }

            if (string.IsNullOrWhiteSpace(request.OrderCode))

            {
                return Result<PaymentStatusDto>.Failure("OrderCode không hợp lệ.");
            }

            var payment = await _context.Payments
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.PayOSOrderCode == request.OrderCode &&
                         x.UserId == currentUserId.Value,
                    cancellationToken);

            if (payment == null)

            {
                return Result<PaymentStatusDto>.Failure(
                    "Không tìm thấy đơn thanh toán.");
            }

            var payOSInfo = await _payOSService.GetPaymentInfoAsync(
                request.OrderCode,
                cancellationToken);

            if (payOSInfo == null)

            {
                return Result<PaymentStatusDto>.Failure(
                    "Không lấy được trạng thái thanh toán từ payOS.");
            }

            payment.LastPayOSStatus = payOSInfo.Status;

            payment.UpdatedAt = DateTime.UtcNow;

            var normalizedStatus = NormalizePayOSStatus(payOSInfo.Status);

            if (normalizedStatus == PaymentStatuses.Paid)

            {

                await _paymentStatusService.MarkPaidAndActivateSubscriptionAsync(

                    payment,

                    rawWebhookJson: null,

                    payOSStatus: payOSInfo.Status,

                    cancellationToken);

            }

            else if (normalizedStatus == PaymentStatuses.Cancelled)

            {

                await _paymentStatusService.MarkCancelledAsync(

                    payment,

                    cancellationToken);

            }

            else if (normalizedStatus == PaymentStatuses.Expired)

            {

                await _paymentStatusService.MarkExpiredAsync(

                    payment,

                    cancellationToken);

            }

            else if (payment.Status == PaymentStatuses.Pending &&

                     payment.ExpiresAt.HasValue &&

                     payment.ExpiresAt.Value < DateTime.UtcNow)

            {

                await _paymentStatusService.MarkExpiredAsync(

                    payment,

                    cancellationToken);

            }

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<PaymentStatusDto>.Success(
                PaymentDtoMapper.ToStatusDto(payment));
        }

        catch (Exception ex)

        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Lỗi khi đồng bộ trạng thái thanh toán {OrderCode}",
                request.OrderCode);

            return Result<PaymentStatusDto>.Failure(
                $"Đã xảy ra lỗi khi đồng bộ trạng thái thanh toán: {ex.Message}");
        }
    }

    private static string NormalizePayOSStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return PaymentStatuses.Pending;
        }

        return status.ToUpperInvariant() switch
        {
            "PAID" => PaymentStatuses.Paid,
            "CANCELLED" => PaymentStatuses.Cancelled,
            "CANCELED" => PaymentStatuses.Cancelled,
            "EXPIRED" => PaymentStatuses.Expired,
            "FAILED" => PaymentStatuses.Failed,
            "PENDING" => PaymentStatuses.Pending,
            _ => PaymentStatuses.Pending
        };
    }
}