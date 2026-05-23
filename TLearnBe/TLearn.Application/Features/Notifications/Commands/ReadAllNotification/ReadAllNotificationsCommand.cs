using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Notifications.Commands.ReadAllNotification;

public record ReadAllNotificationsCommand : IRequest<Result<bool>>;