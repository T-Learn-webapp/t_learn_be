using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.RemoveMember;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<RemoveMemberCommandHandler> _logger;

    public RemoveMemberCommandHandler(TLearnDbContext context, ILogger<RemoveMemberCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

            if (subject == null)
                return Result<bool>.Failure($"Subject with id '{request.SubjectId}' was not found.");

            // Only owner or manager can remove members
            if (subject.UserId != request.CurrentUserId && !subject.CanUserManage(request.CurrentUserId))
                return Result<bool>.Failure("You don't have permission to remove members from this subject.");

            var member = await _context.SubjectMembers
                .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.SubjectId == request.SubjectId, cancellationToken);

            if (member == null)
                return Result<bool>.Failure("Member not found in this subject.");

            // Cannot remove owner
            if (member.UserId == subject.UserId)
                return Result<bool>.Failure("Cannot remove the owner from the subject.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _context.SubjectMembers.Remove(member);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while removing member");
                throw new SqlException("Failed to remove member", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while removing member");
            return Result<bool>.Failure("An unexpected error occurred while removing member.");
        }
    }
}