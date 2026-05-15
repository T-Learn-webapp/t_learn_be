using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Commands.CreateMaterial;

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, Result<LearningMaterialDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<CreateMaterialCommandHandler> _logger;

    public CreateMaterialCommandHandler(TLearnDbContext context, ILogger<CreateMaterialCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<LearningMaterialDto>> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<LearningMaterialDto>.Failure("Material title is required.");

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

            if (subject == null)
                return Result<LearningMaterialDto>.Failure($"Subject with id '{request.SubjectId}' was not found.");

            if (!subject.IsPublic && subject.UserId != request.UserId)
                return Result<LearningMaterialDto>.Failure("You don't have permission to add material to this subject.");

            var material = new LearningMaterial
            {
                Title = request.Title.Trim(),
                Content = request.Content,
                Summary = request.Summary,
                SubjectId = request.SubjectId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.LearningMaterials.Add(material);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<LearningMaterialDto>.Success(new LearningMaterialDto
                {
                    Id = material.Id,
                    Title = material.Title,
                    Content = material.Content,
                    Summary = material.Summary,
                    SubjectId = material.SubjectId,
                    SubjectName = subject.Name,
                    FlashcardCount = 0,
                    CreatedAt = material.CreatedAt,
                    UpdatedAt = material.UpdatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while creating material for subject {SubjectId}", request.SubjectId);
                throw new SqlException("Failed to create material", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while creating material");
            return Result<LearningMaterialDto>.Failure("An unexpected error occurred while creating the material.");
        }
    }
}