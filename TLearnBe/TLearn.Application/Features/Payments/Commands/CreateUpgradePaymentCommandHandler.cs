using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;
using TLearn.Domain.Constants;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;
using TLearn.Infrastructure.Services.PayOs;

namespace TLearn.Application.Features.Payments.Commands;

public class CreateUpgradePaymentCommandHandler
    : IRequestHandler<CreateUpgradePaymentCommand, Result<CreateUpgradePaymentDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IPayOSService _payOSService;

    private readonly PayOSOptions _payOSOptions;

    private readonly ILogger<CreateUpgradePaymentCommandHandler> _logger;

    public CreateUpgradePaymentCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IPayOSService payOSService,
        IOptions<PayOSOptions> payOSOptions,
        ILogger<CreateUpgradePaymentCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _payOSService = payOSService;

        _payOSOptions = payOSOptions.Value;

        _logger = logger;
    }

    public async Task<Result<CreateUpgradePaymentDto>> Handle(
        CreateUpgradePaymentCommand request,
        CancellationToken cancellationToken)

    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken);

        try

        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<CreateUpgradePaymentDto>.Failure("Chưa đăng nhập.");
            }

            if (string.IsNullOrWhiteSpace(request.PlanType))

            {
                return Result<CreateUpgradePaymentDto>.Failure("Vui lòng chọn gói nâng cấp.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == currentUserId.Value &&
                         x.IsActive,
                    cancellationToken);

            if (user == null)

            {
                return Result<CreateUpgradePaymentDto>.Failure(
                    "Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            var plan = await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.PlanType == request.PlanType &&
                         x.IsActive,
                    cancellationToken);

            if (plan == null)

            {
                return Result<CreateUpgradePaymentDto>.Failure(
                    "Gói nâng cấp không tồn tại hoặc đã bị tạm ngừng.");
            }

            if (plan.Amount <= 0)

            {
                return Result<CreateUpgradePaymentDto>.Failure(
                    "Giá gói nâng cấp không hợp lệ.");
            }

            if (plan.DurationDays <= 0)

            {
                return Result<CreateUpgradePaymentDto>.Failure(
                    "Thời hạn gói nâng cấp không hợp lệ.");
            }

            var now = DateTime.UtcNow;

            var orderCode = await GenerateUniqueOrderCodeAsync(cancellationToken);

            var expiresAt = now.AddMinutes(15);

            var payment = new Payment

            {
                UserId = user.Id,

                Amount = plan.Amount,

                Currency = plan.Currency,

                PlanType = plan.PlanType,

                PlanName = plan.PlanName,

                DurationDays = plan.DurationDays,

                PayOSOrderCode = orderCode.ToString(),

                Status = PaymentStatuses.Pending,

                Description = plan.Description ?? $"Thanh toán {plan.PlanName}",

                ExpiresAt = expiresAt,

                CreatedAt = now
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync(cancellationToken);

            var payOSResult = await _payOSService.CreatePaymentLinkAsync(
                new CreatePayOSPaymentRequest

                {
                    OrderCode = orderCode,

                    Amount = Convert.ToInt32(plan.Amount),

                    Description = BuildPayOSDescription(plan.PlanName),

                    ReturnUrl = BuildReturnUrl(orderCode),

                    CancelUrl = BuildCancelUrl(orderCode),

                    Items = new List<CreatePayOSPaymentItem>

                    {
                        new()

                        {
                            Name = plan.PlanName,

                            Quantity = 1,

                            Price = Convert.ToInt32(plan.Amount)
                        }
                    }
                },
                cancellationToken);

            payment.PayOSPaymentLinkId = payOSResult.PaymentLinkId;

            payment.LastPayOSStatus = payOSResult.Status;

            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Result<CreateUpgradePaymentDto>.Success(
                new CreateUpgradePaymentDto

                {
                    PaymentId = payment.Id,

                    OrderCode = payment.PayOSOrderCode,

                    PaymentLinkId = payment.PayOSPaymentLinkId,

                    CheckoutUrl = payOSResult.CheckoutUrl,

                    QrCode = payOSResult.QrCode,

                    Amount = payment.Amount,

                    Currency = payment.Currency,

                    PlanType = payment.PlanType,

                    PlanName = payment.PlanName,

                    ExpiresAt = payment.ExpiresAt
                });
        }

        catch (Exception ex)

        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Lỗi khi tạo link thanh toán nâng cấp tài khoản");

            return Result<CreateUpgradePaymentDto>.Failure(
                $"Đã xảy ra lỗi khi tạo thanh toán: {ex.Message}");
        }
    }

    private async Task<long> GenerateUniqueOrderCodeAsync(
        CancellationToken cancellationToken)

    {
        for (var i = 0; i < 5; i++)

        {
            var orderCode = long.Parse(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    .ToString()[^10..]);

            orderCode += Random.Shared.Next(10, 99);

            var exists = await _context.Payments
                .AnyAsync(
                    x => x.PayOSOrderCode == orderCode.ToString(),
                    cancellationToken);

            if (!exists)

            {
                return orderCode;
            }

            await Task.Delay(50, cancellationToken);
        }

        throw new InvalidOperationException(
            "Không thể tạo mã đơn hàng duy nhất.");
    }

    private string BuildPayOSDescription(string planName)

    {
        var description = $"Nang cap {planName}";

        return description.Length <= 25
            ? description
            : description[..25];
    }

    private string BuildReturnUrl(long orderCode)

    {
        var separator = _payOSOptions.ReturnUrl.Contains('?') ? "&" : "?";

        return $"{_payOSOptions.ReturnUrl}{separator}orderCode={orderCode}";
    }

    private string BuildCancelUrl(long orderCode)

    {
        var separator = _payOSOptions.CancelUrl.Contains('?') ? "&" : "?";

        return $"{_payOSOptions.CancelUrl}{separator}orderCode={orderCode}";
    }
}