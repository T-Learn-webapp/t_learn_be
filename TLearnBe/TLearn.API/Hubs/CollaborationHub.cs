using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.API.Hubs;


using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

public class CollaborationHub : Hub
{
    private readonly ILogger<CollaborationHub> _logger;
    private readonly TLearnDbContext _context;
    private static readonly ConcurrentDictionary<string, HashSet<OnlineUser>> _rooms = new();
    public CollaborationHub(

        ILogger<CollaborationHub> logger,

        TLearnDbContext context)

    {

        _logger = logger;

        _context = context;

    }

    // Method để client gọi khi join
    public async Task JoinMaterial(
        string materialId,
        string userId)
    {
        _logger.LogInformation(
            "JoinMaterial called - MaterialId: {MaterialId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
            materialId,
            userId,
            Context.ConnectionId
        );

        // Create room if not exists
        if (!_rooms.ContainsKey(materialId))
        {
            _rooms[materialId] = new HashSet<OnlineUser>();
        }

        // Add user to room
        _rooms[materialId].Add(new OnlineUser
        {
            UserId = userId,
            ConnectionId = Context.ConnectionId
        });

        // SignalR group
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"material-{materialId}"
        );

        // Notify others
        await Clients.GroupExcept(
                $"material-{materialId}",
                Context.ConnectionId
            )
            .SendAsync(
                "UserJoined",
                userId,
                Context.ConnectionId
            );

        // Send full online list
        await Clients.Caller.SendAsync(
            "OnlineUsers",
            _rooms[materialId]
        );

        // Confirm joined
        await Clients.Caller.SendAsync(
            "Joined",
            materialId,
            userId
        );
    }

    public async Task LeaveMaterial(string materialId, string userId)
    {
        _logger.LogInformation("LeaveMaterial called - MaterialId: {MaterialId}, UserId: {UserId}", materialId, userId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"material-{materialId}");
        
        await Clients.Group($"material-{materialId}")
            .SendAsync("UserLeft", userId, Context.ConnectionId);
    }

    public async Task SendOperation(string materialId, string operation)
    {
        _logger.LogDebug("SendOperation called for material {MaterialId}", materialId);
        
        // Broadcast operation to all clients in the group EXCEPT the sender
        await Clients.GroupExcept($"material-{materialId}", Context.ConnectionId)
            .SendAsync("ReceiveOperation", operation, Context.ConnectionId);
    }

    public async Task SaveSnapshot(
        string materialId,
        string snapshot)
    {
        try
        {
            _logger.LogInformation(
                "SaveSnapshot called for material {MaterialId}",
                materialId
            );

            var materialGuid =
                Guid.Parse(materialId);

            var material =
                await _context.LearningMaterials
                    .FindAsync(materialGuid);

            if (material == null)
            {
                await Clients.Caller.SendAsync(
                    "SnapshotSaveFailed",
                    "Material not found"
                );

                return;
            }

            material.Content = snapshot;

            material.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Snapshot saved successfully for material {MaterialId}",
                materialId
            );

            await Clients.Caller.SendAsync(
                "SnapshotSaved",
                materialId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save snapshot for material {MaterialId}",
                materialId
            );

            await Clients.Caller.SendAsync(
                "SnapshotSaveFailed",
                ex.Message
            );
        }
    }

    public async Task RequestSnapshot(
        string materialId)
    {
        var material =
            await _context.LearningMaterials
                .FindAsync(
                    Guid.Parse(materialId)
                );

        if (material == null)
        {
            await Clients.Caller.SendAsync(
                "ReceiveSnapshot",
                ""
            );

            return;
        }

        await Clients.Caller.SendAsync(
            "ReceiveSnapshot",
            material.Content ?? ""
        );
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(

        Exception? exception)

    {

        foreach (var room in _rooms)
        {
            var users = room.Value;
            var removedUsers =

                users

                    .Where(x =>

                        x.ConnectionId ==

                        Context.ConnectionId)

                    .ToList();

            foreach (var user in removedUsers)
            {
                users.Remove(user);

                await Clients.Group(
                        $"material-{room.Key}"
                    )
                    .SendAsync(
                        "UserLeft",
                        user.UserId,

                        user.ConnectionId

                    );

            }
        }
        await base.OnDisconnectedAsync(
            exception
        );

    }
}
public class OnlineUser

{

    public string UserId { get; set; } = default!;

    public string ConnectionId { get; set; } = default!;

}