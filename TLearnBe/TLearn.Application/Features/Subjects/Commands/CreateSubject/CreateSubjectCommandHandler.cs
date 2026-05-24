using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Commands.CreateSubject;

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, Result<SubjectDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<CreateSubjectCommandHandler> _logger;

    public CreateSubjectCommandHandler(TLearnDbContext context, ILogger<CreateSubjectCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SubjectDto>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<SubjectDto>.Failure("Subject name is required.");

            // Check duplicate name for same user
            var exists = await _context.Subjects
                .AnyAsync(s => s.Name == request.Name && s.UserId == request.UserId, cancellationToken);
            
            if (exists)
                return Result<SubjectDto>.Failure("You already have a subject with this name.");

            var subject = new Subject
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<SubjectDto>.Success(new SubjectDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Description = subject.Description,
                   
                    MaterialCount = 0,
                    CreatedAt = subject.CreatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while creating subject for user {UserId}", request.UserId);
                
                if (ex.InnerException?.Message.Contains("UNIQUE") == true)
                    return Result<SubjectDto>.Failure("A subject with this name already exists.");
                
                throw new SqlException("Failed to create subject", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while creating subject for user {UserId}", request.UserId);
            return Result<SubjectDto>.Failure("An unexpected error occurred while creating the subject.");
        }
    }
}