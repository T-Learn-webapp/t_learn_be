using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TLearn.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}