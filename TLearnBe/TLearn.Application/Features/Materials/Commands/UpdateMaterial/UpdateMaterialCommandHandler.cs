using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Commands.UpdateMaterial;

public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, Result<LearningMaterialDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<UpdateMaterialCommandHandler> _logger;

    public UpdateMaterialCommandHandler(TLearnDbContext context, ILogger<UpdateMaterialCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<LearningMaterialDto>> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<LearningMaterialDto>.Failure("Material title is required.");

            var material = await _context.LearningMaterials
                .Include(m => m.Subject)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (material == null)
                return Result<LearningMaterialDto>.Failure($"Material with id '{request.Id}' was not found.");

            if (material.UserId != request.UserId)
                return Result<LearningMaterialDto>.Failure("You don't have permission to update this material.");

            material.Title = request.Title.Trim();
            material.Content = request.Content;
            material.Summary = request.Summary;
            material.UpdatedAt = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.LearningMaterials.Update(material);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var flashcardCount = await _context.Flashcards
                    .CountAsync(f => f.MaterialId == material.Id, cancellationToken);

                return Result<LearningMaterialDto>.Success(new LearningMaterialDto
                {
                    Id = material.Id,
                    Title = material.Title,
                    Content = material.Content,
                    Summary = material.Summary,
                    SubjectId = material.SubjectId,
                    SubjectName = material.Subject.Name,
                    FlashcardCount = flashcardCount,
                    CreatedAt = material.CreatedAt,
                    UpdatedAt = material.UpdatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while updating material {MaterialId}", request.Id);
                throw new SqlException("Failed to update material", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while updating material {MaterialId}", request.Id);
            return Result<LearningMaterialDto>.Failure("An unexpected error occurred while updating the material.");
        }
    }
}
