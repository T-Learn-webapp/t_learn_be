using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.UpdateSubject;

public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, Result<SubjectDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<UpdateSubjectCommandHandler> _logger;

    public UpdateSubjectCommandHandler(TLearnDbContext context, ILogger<UpdateSubjectCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SubjectDto>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<SubjectDto>.Failure("Subject name is required.");

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subject == null)
                return Result<SubjectDto>.Failure($"Subject with id '{request.Id}' was not found.");

            if (subject.UserId != request.UserId)
                return Result<SubjectDto>.Failure("You don't have permission to update this subject.");

            // Check duplicate name (excluding current subject)
            var duplicate = await _context.Subjects
                .AnyAsync(s => s.Name == request.Name && s.UserId == request.UserId && s.Id != request.Id, cancellationToken);
            
            if (duplicate)
                return Result<SubjectDto>.Failure("You already have another subject with this name.");

            subject.Name = request.Name.Trim();
            subject.Description = request.Description;
            

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.Subjects.Update(subject);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var materialCount = await _context.LearningMaterials
                    .CountAsync(m => m.SubjectId == subject.Id, cancellationToken);

                return Result<SubjectDto>.Success(new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Description = subject.Description,
                    
                    MaterialCount = materialCount,
                    CreatedAt = subject.CreatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while updating subject {SubjectId}", request.Id);
                throw new SqlException("Failed to update subject", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while updating subject {SubjectId}", request.Id);
            return Result<SubjectDto>.Failure("An unexpected error occurred while updating the subject.");
        }
    }
}