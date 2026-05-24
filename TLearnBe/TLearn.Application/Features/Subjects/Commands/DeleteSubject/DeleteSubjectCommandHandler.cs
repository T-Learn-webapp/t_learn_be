using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Subjects.Commands.DeleteSubject;

public class DeleteSubjectHandler
    : IRequestHandler<DeleteSubjectCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSubjectHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(
        DeleteSubjectCommand request,
        CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId;

        if (!currentUserId.HasValue)
        {
            return Result<bool>.Failure("Chưa đăng nhập");
        }

        var subject = await _context.Subjects
            .FirstOrDefaultAsync(
                x => x.Id == request.SubjectId,
                ct);

        if (subject == null || subject.IsDeleted)
        {
            return Result<bool>.Failure("Môn học không tồn tại");
        }

        if (subject.UserId != currentUserId.Value)
        {
            return Result<bool>.Failure("Chỉ chủ sở hữu mới có quyền xoá môn học này");
        }

        subject.IsDeleted = true;
        subject.DeletedAt = DateTime.UtcNow;
        subject.DeletedByUserId = currentUserId.Value;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}