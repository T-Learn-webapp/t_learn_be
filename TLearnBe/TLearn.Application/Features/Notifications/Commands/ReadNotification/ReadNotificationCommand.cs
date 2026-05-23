using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Notifications.Commands.ReadNotification;

public record ReadNotificationCommand

    : IRequest<Result<bool>>

{

    public Guid NotificationId { get; init; }

}