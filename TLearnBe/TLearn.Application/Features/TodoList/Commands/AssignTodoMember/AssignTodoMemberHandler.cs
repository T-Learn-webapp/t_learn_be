using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Commands.AssignTodoMember;

public class AssignTodoMemberHandler
    : IRequestHandler<
        AssignTodoMemberCommand,
        Result<TodoItemDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IHubContext<TodoHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    
    private readonly IConfiguration _config;

    public AssignTodoMemberHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IHubContext<TodoHub> hubContext,
        IEmailService emailService,
        IConfiguration config,
        INotificationService notificationService
        )

    {
        _context = context;

        _currentUser = currentUser;

        _hubContext = hubContext;
        _emailService = emailService;
        _notificationService = notificationService;
        _config = config;
    }

    public async Task<Result<TodoItemDto>> Handle(
        AssignTodoMemberCommand request,
        CancellationToken ct)

    {
        using var transaction =
            await _context.Database.BeginTransactionAsync(ct);

        try

        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<TodoItemDto>
                    .Failure("Chưa đăng nhập");
            }

            var todo = await _context.TodoItems
                .Include(x => x.Assignments)
                .Include(x => x.LearningMaterial)
                .ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.TodoId,
                    ct);

            if (todo == null)

            {
                return Result<TodoItemDto>
                    .Failure("Todo không tồn tại");
            }

            // ===== CHECK PERMISSION =====

            var canEdit = todo.LearningMaterial
                .Subject
                .CanUserEdit(currentUserId.Value);

            if (!canEdit)

            {
                return Result<TodoItemDto>
                    .Failure("Bạn không có quyền assign member");
            }

            // ===== VALIDATE MEMBER =====

            var subjectMemberIds = todo.LearningMaterial
                .Subject
                .Members
                .Select(x => x.UserId)
                .ToHashSet();

            var invalidUserIds = request.UserIds
                .Where(x => !subjectMemberIds.Contains(x))
                .ToList();

            if (invalidUserIds.Any())

            {
                return Result<TodoItemDto>
                    .Failure("Có user không thuộc subject");
            }

            // ===== FILTER USER ĐÃ ASSIGN =====

            var existingUserIds = todo.Assignments
                .Select(x => x.UserId)
                .ToHashSet();

            var newUserIds = request.UserIds
                .Distinct()
                .Where(x => !existingUserIds.Contains(x))
                .ToList();

            // ===== ADD ASSIGNMENTS =====

            foreach (var userId in newUserIds)
            {
                todo.Assignments.Add(new TodoAssignment
                {
                    UserId = userId,
                    Status = TodoStatus.Pending,
                    AssignedAt = DateTime.UtcNow
                });

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == userId, ct);

                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    var todoLink =
                        $"{_config["FrontendUrl"]}/todos/{todo.Id}";

                    await _emailService.SendTodoAssignmentEmail(
                        user.Email,
                        todo.Title,
                        todo.LearningMaterial.Subject.Name,
                        todoLink,
                        todo.DueDate,
                        "TLearn",
                        todo.Description);
                }
            }

            // ===== AUTO UPDATE TODO STATUS =====

            var statuses = todo.Assignments
                .Select(x => x.Status)
                .ToList();

            if (statuses.All(x => x == TodoStatus.Pending))

            {
                todo.Status = TodoStatus.Pending;
            }

            else if (statuses.All(x => x == TodoStatus.Completed))

            {
                todo.Status = TodoStatus.Completed;
            }

            else

            {
                todo.Status = TodoStatus.InProgress;
            }

            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            var response = new TodoItemDto

            {
                Id = todo.Id,

                LearningMaterialId = todo.LearningMaterialId,

                Title = todo.Title,

                Description = todo.Description,

                DueDate = todo.DueDate,

                CreatedByUserId = todo.CreatedByUserId,

                AssignedUsers = todo.Assignments
                    .Select(x => new TodoAssignedUserDto

                    {
                        UserId = x.UserId,

                        Status = x.Status
                    })
                    .ToList()
            };
            
            await _notificationService.CreateManyAsync(
                newUserIds,
                "Bạn có công việc mới",
                $"Bạn được assign task: {todo.Title}",
                NotificationType.TodoAssigned,
                $"/todos/{todo.Id}");

            // ===== REALTIME =====

            await _hubContext.Clients
                .Group($"todo-{todo.Id}")
                .SendAsync(
                    "TodoMembersAssigned",
                    new

                    {
                        TodoId = todo.Id,

                        AssignedUserIds = newUserIds,

                        TodoStatus = todo.Status.ToString()
                    },
                    ct);

            return Result<TodoItemDto>
                .Success(response);
        }

        catch (Exception ex)

        {
            await transaction.RollbackAsync(ct);

            return Result<TodoItemDto>
                .Failure(ex.Message);
        }
    }
}