using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;
using TLearn.Domain.Constants;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;
using TLearn.Infrastructure.Services.Payments;

namespace TLearn.Application.Features.Payments.Queries.GetPaymentStatus;

public class GetPaymentStatusQueryHandler
    : IRequestHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IPaymentStatusService _paymentStatusService;

    private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

    public GetPaymentStatusQueryHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IPaymentStatusService paymentStatusService,
        ILogger<GetPaymentStatusQueryHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _paymentStatusService = paymentStatusService;

        _logger = logger;
    }

    public async Task<Result<PaymentStatusDto>> Handle(
        GetPaymentStatusQuery request,
        CancellationToken cancellationToken)

    {
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
                .FirstOrDefaultAsync(
                    x => x.PayOSOrderCode == request.OrderCode &&
                         x.UserId == currentUserId.Value,
                    cancellationToken);

            if (payment == null)

            {
                return Result<PaymentStatusDto>.Failure(
                    "Không tìm thấy đơn thanh toán.");
            }

            if (payment.Status == PaymentStatuses.Pending &&
                payment.ExpiresAt.HasValue &&
                payment.ExpiresAt.Value < DateTime.UtcNow)

            {
                await _paymentStatusService.MarkExpiredAsync(
                    payment,
                    cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<PaymentStatusDto>.Success(
                PaymentDtoMapper.ToStatusDto(payment));
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy trạng thái thanh toán {OrderCode}",
                request.OrderCode);

            return Result<PaymentStatusDto>.Failure(
                "Đã xảy ra lỗi khi lấy trạng thái thanh toán.");
        }
    }
}