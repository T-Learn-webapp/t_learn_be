using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Commands.UpdateTodoItem;

public class UpdateTodoHandler

    : IRequestHandler<UpdateTodoCommand, Result<TodoItemDto>>
{

    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    public UpdateTodoHandler(

        TLearnDbContext context,

        ICurrentUserService currentUser)

    {

        _context = context;

        _currentUser = currentUser;

    }

    public async Task<Result<TodoItemDto>> Handle(

        UpdateTodoCommand request,

        CancellationToken ct)

    {

        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try

        {

            var todo = await _context.TodoItems

                .Include(x => x.Assignments)

                .Include(x => x.LearningMaterial)

                    .ThenInclude(x => x.Subject)

                        .ThenInclude(x => x.Members)

                .FirstOrDefaultAsync(x => x.Id == request.TodoId, ct);

            if (todo == null)

            {

                return Result<TodoItemDto>

                    .Failure("Todo không tồn tại");

            }

            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                
                return Result<TodoItemDto>
                    .Failure("Chưa đăng nhập");
            }

            // Kiểm tra quyền edit

            if (!todo.LearningMaterial.Subject.CanUserEdit(currentUserId.Value))

            {

                return Result<TodoItemDto>

                    .Failure("Bạn không có quyền cập nhật todo");

            }

            // Validate assigned users

            var subjectMemberIds = todo.LearningMaterial.Subject.Members

                .Select(x => x.UserId)

                .ToHashSet();

            if(todo.LearningMaterial.Subject.UserId == currentUserId)
                subjectMemberIds.Add(currentUserId.Value);
            
            var invalidUsers = request.AssignedUserIds

                .Where(x => !subjectMemberIds.Contains(x))

                .ToList();

            if (invalidUsers.Any())

            {

                return Result<TodoItemDto>

                    .Failure("Có thành viên không thuộc subject");

            }

            // Update thông tin

            todo.Title = request.Title;

            todo.Description = request.Description;

            todo.DueDate = request.DueDate;

            todo.UpdatedAt = DateTime.UtcNow;

            // ===== Update assignments =====

            var existingUserIds = todo.Assignments

                .Select(x => x.UserId)

                .ToHashSet();

            var newUserIds = request.AssignedUserIds

                .Distinct()

                .ToHashSet();

            // Remove users

            var removeAssignments = todo.Assignments

                .Where(x => !newUserIds.Contains(x.UserId))

                .ToList();

            _context.TodoAssignments.RemoveRange(removeAssignments);

            // Add users

            var addUserIds = newUserIds

                .Where(x => !existingUserIds.Contains(x))

                .ToList();

            foreach (var userId in addUserIds)

            {

                todo.Assignments.Add(new TodoAssignment

                {

                    UserId = userId,

                    Status = TodoStatus.Pending

                });

            }

            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return Result<TodoItemDto>.Success(new TodoItemDto

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

            });

        }

        catch (Exception ex)

        {

            await transaction.RollbackAsync(ct);

            return Result<TodoItemDto>

                .Failure($"Lỗi cập nhật Todo: {ex.Message}");

        }

    }

}