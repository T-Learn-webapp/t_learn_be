using Microsoft.AspNetCore.SignalR;

namespace TLearn.Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;

public class SubjectHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"user-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}