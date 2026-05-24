using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Queries.GetMaterialById;

public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, Result<LearningMaterialDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetMaterialByIdQueryHandler> _logger;

    public GetMaterialByIdQueryHandler(TLearnDbContext context, ILogger<GetMaterialByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<LearningMaterialDto>> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var material = await _context.LearningMaterials
                .Include(m => m.Subject)
                .Include(m => m.Flashcards)
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.IsDeleted == false, cancellationToken);

            if (material == null)
                return Result<LearningMaterialDto>.Failure($"Material with id '{request.Id}' was not found.");

            // Check permission: user can view if:
            // 1. The subject is public, OR
            // 2. User owns the subject, OR
            // 3. User owns the material
            if(material.Subject.CanUserView(request.UserId.Value))
                return Result<LearningMaterialDto>.Failure("You don't have permission to view this material.");
            

            return Result<LearningMaterialDto>.Success(new LearningMaterialDto
            {
                Id = material.Id,
                Title = material.Title,
                Content = material.Content,
                Summary = material.Summary,
                SubjectId = material.SubjectId,
                SubjectName = material.Subject.Name,
                FlashcardCount = material.Flashcards.Count,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting material {MaterialId}", request.Id);
            return Result<LearningMaterialDto>.Failure("An error occurred while retrieving the material.");
        }
    }
}