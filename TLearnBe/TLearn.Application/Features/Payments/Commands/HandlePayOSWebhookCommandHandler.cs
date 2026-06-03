using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Constants;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services.Payments;
using TLearn.Infrastructure.Services.PayOs;

namespace TLearn.Application.Features.Payments.Commands;

public class HandlePayOSWebhookCommandHandler
    : IRequestHandler<HandlePayOSWebhookCommand, Result<bool>>

{
    private readonly TLearnDbContext _context;

    private readonly IPayOSService _payOSService;

    private readonly IPaymentStatusService _paymentStatusService;

    private readonly ILogger<HandlePayOSWebhookCommandHandler> _logger;

    public HandlePayOSWebhookCommandHandler(
        TLearnDbContext context,
        IPayOSService payOSService,
        IPaymentStatusService paymentStatusService,
        ILogger<HandlePayOSWebhookCommandHandler> logger)

    {
        _context = context;

        _payOSService = payOSService;

        _paymentStatusService = paymentStatusService;

        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        HandlePayOSWebhookCommand request,
        CancellationToken cancellationToken)

    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        try

        {
            var webhookData = _payOSService.VerifyWebhook(request.RawBody);

            var orderCode = webhookData.OrderCode.ToString();

            var payment = await _context.Payments
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.PayOSOrderCode == orderCode,
                    cancellationToken);

            if (payment == null)

            {
                _logger.LogWarning(
                    "Webhook payOS gửi về nhưng không tìm thấy Payment. OrderCode: {OrderCode}",
                    orderCode);

                return Result<bool>.Failure(
                    $"Không tìm thấy đơn thanh toán {orderCode}.");
            }

            payment.WebhookReceivedCount += 1;

            payment.RawWebhookJson = request.RawBody;

            payment.LastPayOSStatus = webhookData.Code;

            payment.UpdatedAt = DateTime.UtcNow;

            /*

             * payOS thường trả code = "00" khi giao dịch thành công.

             * Nếu test thực tế payload khác, mình sẽ chỉnh mapping status lại.

             */

            if (webhookData.Code == "00")

            {
                await _paymentStatusService.MarkPaidAndActivateSubscriptionAsync(
                    payment,
                    request.RawBody,
                    PaymentStatuses.Paid,
                    cancellationToken);
            }

            else

            {
                payment.Status = PaymentStatuses.Failed;

                payment.LastPayOSStatus = webhookData.Code;

                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        catch (Exception ex)

        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Lỗi khi xử lý webhook payOS");

            return Result<bool>.Failure(
                $"Lỗi xử lý webhook payOS: {ex.Message}");
        }
    }
}