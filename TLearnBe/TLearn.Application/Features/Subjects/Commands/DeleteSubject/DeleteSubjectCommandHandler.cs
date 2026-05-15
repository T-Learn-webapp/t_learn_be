using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.DeleteSubject;

public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<DeleteSubjectCommandHandler> _logger;

    public DeleteSubjectCommandHandler(TLearnDbContext context, ILogger<DeleteSubjectCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .Include(s => s.Materials)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subject == null)
                return Result<bool>.Failure($"Subject with id '{request.Id}' was not found.");

            if (subject.UserId != request.UserId)
                return Result<bool>.Failure("You don't have permission to delete this subject.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while deleting subject {SubjectId}", request.Id);
                
                if (ex.InnerException?.Message.Contains("REFERENCE") == true)
                    return Result<bool>.Failure("Cannot delete subject because it has associated materials. Please delete them first.");
                
                throw new SqlException("Failed to delete subject", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while deleting subject {SubjectId}", request.Id);
            return Result<bool>.Failure("An unexpected error occurred while deleting the subject.");
        }
    }
}