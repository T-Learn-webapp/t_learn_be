using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Subjects.Commands.RemoveMember;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<RemoveMemberCommandHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public RemoveMemberCommandHandler(TLearnDbContext context, ILogger<RemoveMemberCommandHandler> logger,
        ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (currentUserId == null)
            {
                return Result<bool>.Failure("Chưa đăng nhập");
            }

            var subject = await _context.Subjects
                .Include(s => s.Members)
                .FirstOrDefaultAsync(
                    s => s.Id == request.SubjectId && !s.IsDeleted,
                    cancellationToken);

            if (subject == null)
            {
                return Result<bool>.Failure("Môn học không tồn tại.");
            }

            // Chỉ chủ sở hữu hoặc người có quyền quản lý mới được xoá thành viên
            if (subject.UserId != currentUserId.Value &&
                !subject.CanUserManage(currentUserId.Value))
            {
                return Result<bool>.Failure(
                    "Bạn không có quyền xoá thành viên khỏi môn học này.");
            }

            var member = await _context.SubjectMembers
                .FirstOrDefaultAsync(
                    m => m.Id == request.MemberId &&
                         m.SubjectId == request.SubjectId &&
                         !m.IsDeleted,
                    cancellationToken);

            if (member == null)
            {
                return Result<bool>.Failure(
                    "Không tìm thấy thành viên trong môn học này.");
            }

            // Không được xoá chủ sở hữu
            if (member.UserId == subject.UserId)
            {
                return Result<bool>.Failure(
                    "Không thể xoá chủ sở hữu khỏi môn học.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                member.IsDeleted = true;
                member.DeletedAt = DateTime.UtcNow;
                member.DeletedByUserId = currentUserId.Value;

                var todoAssignments = await _context.TodoAssignments
                    .Where(a =>
                        a.UserId == member.UserId &&
                        a.TodoItem.LearningMaterial.SubjectId == request.SubjectId &&
                        !a.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var assignment in todoAssignments)
                {
                    assignment.IsDeleted = true;
                    assignment.DeletedAt = DateTime.UtcNow;
                    assignment.DeletedByUserId = currentUserId.Value;
                }
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Lỗi database khi xoá thành viên khỏi môn học");

                throw new SqlException("Không thể xoá thành viên khỏi môn học.", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(
                ex,
                "Lỗi không xác định khi xoá thành viên khỏi môn học");

            return Result<bool>.Failure(
                "Đã xảy ra lỗi khi xoá thành viên khỏi môn học.");
        }
    }
}