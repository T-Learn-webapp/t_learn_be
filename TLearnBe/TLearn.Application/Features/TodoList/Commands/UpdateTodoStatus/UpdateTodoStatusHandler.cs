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

namespace TLearn.Application.Features.TodoList.Commands.UpdateTodoStatus;

public class UpdateTodoAssignmentStatusHandler
    : IRequestHandler<
        UpdateTodoAssignmentStatusCommand,
        Result<TodoItemDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IHubContext<TodoHub> _hubContext;
    private readonly INotificationService _notificationService;

    public UpdateTodoAssignmentStatusHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IHubContext<TodoHub> hubContext,
        INotificationService notificationService
    )

    {
        _context = context;

        _currentUser = currentUser;

        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    public async Task<Result<TodoItemDto>> Handle(
        UpdateTodoAssignmentStatusCommand request,
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

            if (!Enum.TryParse<TodoStatus>(
                    request.Status,
                    true,
                    out var parsedStatus))
            {
                return Result<TodoItemDto>
                    .Failure("Trạng thái không hợp lệ");
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

            // Assignment cần update

            var assignment = todo.Assignments
                .FirstOrDefault(x =>
                    x.UserId == request.AssignedUserId);

            if (assignment == null)

            {
                return Result<TodoItemDto>
                    .Failure("Assignment không tồn tại");
            }

            // ===== PHÂN QUYỀN =====

            var canEditAll = todo.LearningMaterial
                .Subject
                .CanUserEdit(currentUserId.Value);

            var isOwnAssignment =
                assignment.UserId == currentUserId.Value;

            // Không phải owner/admin

            // và cũng không phải assignment của mình

            if (!canEditAll && !isOwnAssignment)

            {
                return Result<TodoItemDto>
                    .Failure("Bạn không có quyền cập nhật");
            }

            // ===== UPDATE ASSIGNMENT =====

            assignment.Status = parsedStatus;

            if (parsedStatus == TodoStatus.Completed)

            {
                assignment.CompletedAt = DateTime.UtcNow;
            }

            else

            {
                assignment.CompletedAt = null;
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

            var isUpdatedByOwnerOrManager =
                canEditAll && !isOwnAssignment;

            // Thành viên tự update task của mình

            if (!isUpdatedByOwnerOrManager)

            {
                // Gửi cho owner task

                if (todo.CreatedByUserId != currentUserId.Value)

                {
                    await _notificationService.CreateAsync(
                        todo.CreatedByUserId,
                        "Thành viên cập nhật tiến độ công việc",
                        $"Task \"{todo.Title}\" đã được cập nhật sang trạng thái {assignment.Status}",
                        NotificationType.TodoUpdated,
                        $"/todos/{todo.Id}");
                }

                // Gửi cho manager/editors khác trong subject

                var managerIds = todo.LearningMaterial
                    .Subject
                    .Members
                    .Where(x =>
                        x.UserId != currentUserId.Value &&
                        x.UserId != todo.CreatedByUserId &&
                        (
                            x.Permission == SubjectPermission.Edit ||
                            x.Permission == SubjectPermission.Manage
                        ))
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToList();

                if (managerIds.Any())

                {
                    await _notificationService.CreateManyAsync(
                        managerIds,
                        "Tiến độ task vừa thay đổi",
                        $"Task \"{todo.Title}\" vừa được cập nhật sang trạng thái {assignment.Status}",
                        NotificationType.TodoUpdated,
                        $"/todos/{todo.Id}");
                }
            }

            else

            {
                // Owner/Admin sửa trạng thái member khác

                await _notificationService.CreateAsync(
                    assignment.UserId,
                    "Trạng thái công việc của bạn đã được cập nhật",
                    $"Task \"{todo.Title}\" đã được cập nhật sang trạng thái {assignment.Status}",
                    NotificationType.TodoUpdated,
                    $"/todos/{todo.Id}");
            }
            // ===== REALTIME =====

            await _hubContext.Clients
                .Group($"todo-{todo.Id}")
                .SendAsync(
                    "TodoAssignmentUpdated",
                    new
                    {
                        TodoId = todo.Id,
                        TodoStatus = todo.Status.ToString(),
                        AssignmentUserId = assignment.UserId,
                        AssignmentStatus = assignment.Status.ToString()
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