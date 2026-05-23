using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Notifications.Commands.ReadNotification;

public class ReadNotificationHandler
    : IRequestHandler<
        ReadNotificationCommand,
        Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ReadNotificationHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)

    {
        _context = context;

        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(
        ReadNotificationCommand request,
        CancellationToken ct)

    {
        var currentUserId = _currentUser.UserId;

        if (!currentUserId.HasValue)

        {
            return Result<bool>
                .Failure("Chưa đăng nhập");
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.NotificationId
                    && x.UserId == currentUserId.Value,
                ct);

        if (notification == null)

        {
            return Result<bool>
                .Failure("Thông báo không tồn tại");
        }

        if (!notification.IsRead)

        {
            notification.IsRead = true;

            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        return Result<bool>.Success(true);
    }
}