using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.Notifications.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Notifications.Queries;

public class GetMyNotificationsHandler
    : IRequestHandler<
        GetMyNotificationsQuery,
        Result<PagedResult<NotificationDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)

    {
        _context = context;

        _currentUser = currentUser;
    }

    public async Task<
        Result<PagedResult<NotificationDto>>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken ct)

    {
        var currentUserId = _currentUser.UserId;

        if (!currentUserId.HasValue)

        {
            return Result<PagedResult<NotificationDto>>
                .Failure("Chưa đăng nhập");
        }

        var query = _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId.Value);

        // Filter IsRead

        if (request.IsRead.HasValue)

        {
            query = query.Where(x =>
                x.IsRead == request.IsRead.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1)
                  * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new NotificationDto

            {
                Id = x.Id,

                Title = x.Title,

                Message = x.Message,

                Type = x.Type.ToString(),

                IsRead = x.IsRead,

                ActionUrl = x.ActionUrl,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        var result = new PagedResult<NotificationDto>

        {
            Items = notifications,

            PageNumber = request.PageNumber,

            PageSize = request.PageSize,

            TotalCount = totalCount
        };

        return Result<PagedResult<NotificationDto>>
            .Success(result);
    }
}