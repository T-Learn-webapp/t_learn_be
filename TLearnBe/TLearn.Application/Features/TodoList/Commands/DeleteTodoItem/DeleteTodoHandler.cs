using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Commands.DeleteTodoItem;

public class DeleteTodoHandler
    : IRequestHandler<DeleteTodoCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IHubContext<TodoHub> _hubContext;
    public DeleteTodoHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IHubContext<TodoHub> hubContext)
    {
        _context = context;
        _currentUser = currentUser;
        _hubContext = hubContext;
    }

    public async Task<Result<bool>> Handle(
        DeleteTodoCommand request,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;

        if (!currentUserId.HasValue)
        {
            return Result<bool>.Failure("Chưa đăng nhập");
        }

        var todo = await _context.TodoItems
            .Include(x => x.LearningMaterial)
            .ThenInclude(x => x.Subject)
            .ThenInclude(x => x.Members)
            .FirstOrDefaultAsync(
                x => x.Id == request.TodoId,
                ct);

        if (todo == null || todo.IsDeleted)
        {
            return Result<bool>.Failure("Todo không tồn tại");
        }

        if (!todo.LearningMaterial.Subject.CanUserEdit(currentUserId.Value))
        {
            return Result<bool>.Failure("Bạn không có quyền xoá todo này");
        }

        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;
        todo.DeletedByUserId = currentUserId.Value;
        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var subjectUserIds = todo.LearningMaterial.Subject.Members
            .Select(x => x.UserId)
            .ToHashSet();

        subjectUserIds.Add(todo.LearningMaterial.Subject.UserId);

        foreach (var userId in subjectUserIds)
        {
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync(
                    "TodoDeleted",
                    new
                    {
                        TodoId = todo.Id,
                        LearningMaterialId = todo.LearningMaterialId,
                        SubjectId = todo.LearningMaterial.Subject.Id
                    },
                    ct);
        }
        
        return Result<bool>.Success(true);
    }
}