using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.UpdateMemberPermission;

public class UpdateMemberPermissionCommandHandler : IRequestHandler<UpdateMemberPermissionCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<UpdateMemberPermissionCommandHandler> _logger;

    public UpdateMemberPermissionCommandHandler(TLearnDbContext context, ILogger<UpdateMemberPermissionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateMemberPermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

            if (subject == null)
                return Result<bool>.Failure($"Subject with id '{request.SubjectId}' was not found.");

            // Only owner or manager can update permissions
            if (subject.UserId != request.CurrentUserId && !subject.CanUserManage(request.CurrentUserId))
                return Result<bool>.Failure("You don't have permission to update member permissions.");

            var member = await _context.SubjectMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.SubjectId == request.SubjectId, cancellationToken);

            if (member == null)
                return Result<bool>.Failure("Member not found in this subject.");

            // Cannot change owner's permission
            if (member.UserId == subject.UserId)
                return Result<bool>.Failure("Cannot change the owner's permissions.");

            if (!Enum.TryParse<SubjectPermission>(request.Permission, true, out var permission))
                return Result<bool>.Failure($"Invalid permission. Allowed: ViewOnly, Comment, Edit, Manage");

            member.Permission = permission;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _context.SubjectMembers.Update(member);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while updating member permission");
                throw new SqlException("Failed to update member permission", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while updating member permission");
            return Result<bool>.Failure("An unexpected error occurred while updating permission.");
        }
    }
}