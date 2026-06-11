using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TLearn.Infrastructure.Hubs;

[Authorize(Policy = "AdminOnly")]
public class AdminHub : Hub

{
    public override async Task OnConnectedAsync()

    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            "admins");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)

    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            "admins");

        await base.OnDisconnectedAsync(exception);
    }
}