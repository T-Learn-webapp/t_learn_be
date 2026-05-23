using MediatR;
using TLearn.Application.Features.Notifications.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery
    : IRequest<Result<PagedResult<NotificationDto>>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public bool? IsRead { get; init; }
}