using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.API.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

[Authorize]
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
    public async Task JoinMaterial(string materialId)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId) ||
            !Guid.TryParse(userId, out var userGuid) ||
            !Guid.TryParse(materialId, out var materialGuid))
        {
            throw new HubException("Thông tin kết nối không hợp lệ");
        }

        var material = await _context.LearningMaterials
            .Include(x => x.Subject)
            .ThenInclude(x => x.Members)
            .FirstOrDefaultAsync(x =>
                x.Id == materialGuid &&
                !x.IsDeleted);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userGuid);

        if (user == null)
        {
            throw new HubException("Người dùng không tồn tại");
        }

        if (material == null)
        {
            throw new HubException("Tài liệu không tồn tại");
        }

        if (!material.Subject.CanUserView(userGuid))
        {
            throw new HubException("Bạn không có quyền xem tài liệu này");
        }

        var roomKey = materialId;

        var users = _rooms.GetOrAdd(roomKey, _ => new HashSet<OnlineUser>());

        lock (users)
        {
            users.RemoveWhere(x => x.ConnectionId == Context.ConnectionId);

            users.Add(new OnlineUser
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                UserName = user.FullName,
                UserEmail = user.Email,
                UserColor = GetUserColor(userId)
            });
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"material-{materialId}");

        await Clients.GroupExcept(
                $"material-{materialId}",
                Context.ConnectionId)
            .SendAsync(
                "UserJoined",
                new OnlineUser
                {
                    UserId = userId,
                    ConnectionId = Context.ConnectionId,
                    UserName = user.FullName,
                    UserEmail = user.Email,
                    UserColor = GetUserColor(userId)
                });

        await Clients.Caller.SendAsync(
            "OnlineUsers",
            users.ToList());

        await Clients.Caller.SendAsync(
            "Joined",
            materialId,
            new OnlineUser
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                UserName = user.FullName,
                UserEmail = user.Email,
                UserColor = GetUserColor(userId)
            });
    }

    public async Task LeaveMaterial(string materialId)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        _logger.LogInformation(
            "LeaveMaterial called - MaterialId: {MaterialId}, UserId: {UserId}",
            materialId,
            userId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"material-{materialId}");

        if (_rooms.TryGetValue(materialId, out var users))
        {
            lock (users)
            {
                users.RemoveWhere(x => x.ConnectionId == Context.ConnectionId);
            }
        }

        await Clients.Group($"material-{materialId}")
            .SendAsync("UserLeft", userId, Context.ConnectionId);
    }

    public async Task SendOperation(string materialId, string operation)
    {
        _logger.LogDebug("SendOperation called for material {MaterialId}", materialId);

        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("Người dùng không hợp lệ");
        }

        await Clients.GroupExcept($"material-{materialId}", Context.ConnectionId)
            .SendAsync(
                "ReceiveOperation",
                new
                {
                    Operation = operation,
                    UserId = userId,
                    ConnectionId = Context.ConnectionId,
                    UserColor = GetUserColor(userId)
                });
    }

    public async Task SaveSnapshot(
        string materialId,
        string snapshot)
    {
        try
        {
            _logger.LogInformation(
                "SaveSnapshot called for material {MaterialId}",
                materialId);

            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId) ||
                !Guid.TryParse(userId, out var userGuid))
            {
                await Clients.Caller.SendAsync(
                    "SnapshotSaveFailed",
                    "Người dùng không hợp lệ");

                return;
            }

            if (!Guid.TryParse(materialId, out var materialGuid))
            {
                await Clients.Caller.SendAsync(
                    "SnapshotSaveFailed",
                    "MaterialId không hợp lệ");

                return;
            }

            var material = await _context.LearningMaterials
                .Include(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(x =>
                    x.Id == materialGuid &&
                    !x.IsDeleted);

            if (material == null)
            {
                await Clients.Caller.SendAsync(
                    "SnapshotSaveFailed",
                    "Không tìm thấy tài liệu");

                return;
            }

            if (!material.Subject.CanUserEdit(userGuid))
            {
                await Clients.Caller.SendAsync(
                    "SnapshotSaveFailed",
                    "Bạn không có quyền chỉnh sửa tài liệu này");

                return;
            }

            var nextVersion = material.Version + 1;
            var contributors = GetRoomContributors(materialId);

            var history = new LearningMaterialVersion
            {
                LearningMaterialId = material.Id,
                VersionNumber = nextVersion,
                Title = material.Title,
                Content = snapshot,
                Summary = material.Summary,
                YjsSnapshot = snapshot,
                EditedByUserId = userGuid,
                ContributorsJson = JsonSerializer.Serialize(contributors),
                CreatedAt = DateTime.UtcNow,
                ChangeNote = "Lưu nội dung từ cộng tác realtime"
            };

            _context.LearningMaterialVersions.Add(history);

            material.Content = snapshot;
            material.YjsSnapshot = snapshot;
            material.Version = nextVersion;
            material.LastSyncedAt = DateTime.UtcNow;
            material.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await Clients.Caller.SendAsync(
                "SnapshotSaved",
                materialId,
                nextVersion);

            await Clients.GroupExcept(
                    $"material-{materialId}",
                    Context.ConnectionId)
                .SendAsync(
                    "MaterialVersionUpdated",
                    new
                    {
                        MaterialId = material.Id,
                        Version = nextVersion,
                        UpdatedAt = material.UpdatedAt,
                        UpdatedByUserId = userGuid,
                        Contributors = contributors
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save snapshot for material {MaterialId}",
                materialId);

            await Clients.Caller.SendAsync(
                "SnapshotSaveFailed",
                "Không thể lưu nội dung tài liệu");
        }
    }

    public async Task RequestSnapshot(string materialId)
    {
        if (!Guid.TryParse(materialId, out var materialGuid))
        {
            await Clients.Caller.SendAsync("ReceiveSnapshot", "");
            return;
        }

        var material = await _context.LearningMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == materialGuid &&
                !x.IsDeleted);

        if (material == null)
        {
            await Clients.Caller.SendAsync("ReceiveSnapshot", "");
            return;
        }

        await Clients.Caller.SendAsync(
            "ReceiveSnapshot",
            material.YjsSnapshot ?? material.Content ?? "");
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var room in _rooms)
        {
            var materialId = room.Key;
            var users = room.Value;

            List<OnlineUser> removedUsers;

            lock (users)
            {
                removedUsers = users
                    .Where(x => x.ConnectionId == Context.ConnectionId)
                    .ToList();

                foreach (var user in removedUsers)
                {
                    users.Remove(user);
                }
            }

            foreach (var user in removedUsers)
            {
                await Clients.Group($"material-{materialId}")
                    .SendAsync(
                        "UserLeft",
                        user.UserId,
                        user.ConnectionId);
            }

            if (users.Count == 0)
            {
                _rooms.TryRemove(materialId, out _);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static List<OnlineUser> GetRoomContributors(string materialId)
    {
        if (!_rooms.TryGetValue(materialId, out var users))
        {
            return new List<OnlineUser>();
        }

        lock (users)
        {
            return users
                .GroupBy(x => x.UserId)
                .Select(g => g.First())
                .Select(x => new OnlineUser
                {
                    UserId = x.UserId,
                    ConnectionId = x.ConnectionId,
                    UserName = x.UserName,
                    UserEmail = x.UserEmail,
                    UserColor = x.UserColor
                })
                .ToList();
        }
    }

    private static string GetUserColor(string userId)
    {
        var colors = new[]
        {
            "#22C55E",
            "#3B82F6",
            "#F97316",
            "#A855F7",
            "#EF4444",
            "#14B8A6",
            "#EAB308",
            "#EC4899"
        };

        var hash = Math.Abs(userId.GetHashCode());
        return colors[hash % colors.Length];
    }
}

public class OnlineUser

{
    public string UserId { get; set; } = default!;

    public string ConnectionId { get; set; } = default!;
    
    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserColor { get; set; }
}