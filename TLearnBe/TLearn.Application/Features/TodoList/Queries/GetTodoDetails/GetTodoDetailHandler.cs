using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Queries.GetTodoDetails;

public class GetTodoDetailHandler
    : IRequestHandler<GetTodoDetailQuery, Result<TodoDetailDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTodoDetailHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<TodoDetailDto>> Handle(
        GetTodoDetailQuery request,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;

        var todo = await _context.TodoItems
            .AsNoTracking()
            .Where(x => x.Id == request.TodoItemId)
            .Select(x => new TodoDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                DueDate = x.DueDate,

                LearningMaterialId = x.LearningMaterialId,
                LearningMaterialTitle = x.LearningMaterial.Title,

                SubjectId = x.LearningMaterial.Subject.Id,
                SubjectName = x.LearningMaterial.Subject.Name,

                CreatedByUserId = x.CreatedByUserId,
                CreatedByUserName = x.CreatedBy.FullName,

                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                // Status riêng của current user
                Status = x.Assignments
                    .Where(a => a.UserId == currentUserId)
                    .Select(a => a.Status)
                    .FirstOrDefault(),

                AssignedUsers = x.Assignments
                    .Select(a => new TodoAssignedUserDetailDto
                    {
                        UserId = a.UserId,
                        UserName = a.User.FullName,
                        Status = a.Status,
                        AssignedAt = a.AssignedAt,
                        CompletedAt = a.CompletedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (todo == null)
        {
            return Result<TodoDetailDto>.Failure("Todo item not found");
        }

        return Result<TodoDetailDto>.Success(todo);
    }
}