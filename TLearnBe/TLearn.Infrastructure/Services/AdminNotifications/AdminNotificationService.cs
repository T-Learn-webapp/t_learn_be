using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Domain.Constants;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;

namespace TLearn.Infrastructure.Services.AdminNotifications;

public class AdminNotificationService : IAdminNotificationService

{
    private readonly TLearnDbContext _context;

    private readonly UserManager<User> _userManager;

    private readonly IHubContext<AdminHub> _hubContext;

    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        TLearnDbContext context,
        UserManager<User> userManager,
        IHubContext<AdminHub> hubContext,
        ILogger<AdminNotificationService> logger)

    {
        _context = context;

        _userManager = userManager;

        _hubContext = hubContext;

        _logger = logger;
    }

    public async Task NotifyPaymentPaidAsync(
        Payment payment,
        CancellationToken cancellationToken = default)

    {
        try

        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == payment.UserId,
                    cancellationToken);

            if (user == null)

            {
                _logger.LogWarning(
                    "Không tìm thấy user của payment {PaymentId}",
                    payment.Id);

                return;
            }

            var message =
                $"{user.FullName} đã thanh toán thành công gói {payment.PlanName} - {payment.Amount:N0} {payment.Currency}.";

            var payload = new AdminPaymentPaidNotificationDto

            {
                PaymentId = payment.Id,

                OrderCode = payment.PayOSOrderCode,

                UserId = user.Id,

                UserName = user.FullName ?? string.Empty,

                UserEmail = user.Email ?? string.Empty,

                Amount = payment.Amount,

                Currency = payment.Currency,

                PlanType = payment.PlanType,

                PlanName = payment.PlanName,

                PaidAt = payment.PaidAt,

                Message = message
            };

            // 1. Lưu notification cho tất cả admin

            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);

            foreach (var admin in admins)

            {
                var notification = new Notification

                {
                    UserId = admin.Id,

                    Title = "Thanh toán thành công",

                    Message = message,

                    Type = NotificationType.System,

                    IsRead = false,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 2. Bắn realtime về admin đang online

            await _hubContext.Clients
                .Group("admins")
                .SendAsync(
                    "AdminPaymentPaid",
                    payload,
                    cancellationToken);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi gửi notification realtime cho admin. PaymentId: {PaymentId}",
                payment.Id);

            // Không throw để không làm hỏng luồng thanh toán chính
        }
    }
}