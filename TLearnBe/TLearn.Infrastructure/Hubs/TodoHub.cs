using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TLearn.Infrastructure.Hubs;
[Authorize]
public class TodoHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // var userId = Context.UserIdentifier;

        var userId =
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))

        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinTodoGroup(Guid todoId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"todo-{todoId}");
    }

    public async Task LeaveTodoGroup(Guid todoId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"todo-{todoId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // var userId = Context.UserIdentifier;
        var userId =
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))

        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}