using MediatR;
using Microsoft.EntityFrameworkCore;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.TodoList.Commands.DeleteTodoItem;

public class DeleteTodoHandler
    : IRequestHandler<DeleteTodoCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTodoHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(
        DeleteTodoCommand request,
        CancellationToken ct)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var todo = await _context.TodoItems
                .Include(x => x.LearningMaterial)
                .ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == request.TodoId, ct);

            if (todo == null)
            {
                return Result<bool>
                    .Failure("Todo không tồn tại");
            }

            
            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                
                return Result<bool>
                    .Failure("Chưa đăng nhập");
            }

            // Chỉ Edit/Manage mới được xoá
            if (!todo.LearningMaterial.Subject.CanUserEdit(currentUserId.Value))
            {
                return Result<bool>
                    .Failure("Bạn không có quyền xoá todo");
            }

            _context.TodoItems.Remove(todo);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            return Result<bool>
                .Failure($"Lỗi xoá Todo: {ex.Message}");
        }
    }
}