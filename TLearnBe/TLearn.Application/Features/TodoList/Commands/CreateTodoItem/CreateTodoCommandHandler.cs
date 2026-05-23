using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Commands.CreateTodoItem;

public class CreateTodoHandler 

    : IRequestHandler<CreateTodoCommand, Result<TodoItemDto>>

{

    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    public CreateTodoHandler(

        TLearnDbContext context,

        ICurrentUserService currentUser)

    {

        _context = context;

        _currentUser = currentUser;

    }

    public async Task<Result<TodoItemDto>> Handle(

        CreateTodoCommand request,

        CancellationToken ct)

    {

        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try

        {

            // Lấy material + subject + members

            var material = await _context.LearningMaterials
                .Include(x => x.Subject)
                .ThenInclude(x => x.Members)

                .FirstOrDefaultAsync(

                    x => x.Id == request.LearningMaterialId,

                    ct);

            if (material == null)
            {
                return Result<TodoItemDto>
                    .Failure("Learning material không tồn tại");
            }
            // Kiểm tra người tạo có quyền edit/manage không

            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                
                return Result<TodoItemDto>
                    .Failure("Chưa đăng nhập");
            }
            if (!material.Subject.CanUserEdit(currentUserId.Value))
            {
                return Result<TodoItemDto>
                    .Failure("Bạn không có quyền tạo todo");

            }

            // Danh sách member thuộc subject

            var subjectMemberIds = material.Subject.Members
                .Select(x => x.UserId)
                .ToHashSet();

            // Validate assigned users

            var invalidUsers = request.AssignedUserIds
                .Where(x => !subjectMemberIds.Contains(x))
                .ToList();

            if (invalidUsers.Any())
            {
                return Result<TodoItemDto>
                    .Failure("Có thành viên không thuộc subject");
            }
            // Tạo todo

            var todo = new TodoItem
            {
                LearningMaterialId = request.LearningMaterialId,

                Title = request.Title,

                Description = request.Description,

                DueDate = request.DueDate,

                CreatedByUserId = currentUserId.Value

            };

            // Assign nhiều thành viên

            foreach (var userId in request.AssignedUserIds.Distinct())
            {
                todo.Assignments.Add(new TodoAssignment
                {
                    TodoItemId  = todo.Id,
                    UserId = userId,
                    Status = TodoStatus.Pending

                });

            }
            _context.TodoItems.Add(todo);
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
                .Failure($"Lỗi khi tạo Todo: {ex.Message}");

        }

    }

}