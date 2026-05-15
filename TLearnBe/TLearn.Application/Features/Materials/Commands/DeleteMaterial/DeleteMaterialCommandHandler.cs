using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Common;
using TLearn.Domain.Exceptions;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Commands.DeleteMaterial;

public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<DeleteMaterialCommandHandler> _logger;

    public DeleteMaterialCommandHandler(TLearnDbContext context, ILogger<DeleteMaterialCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var material = await _context.LearningMaterials
                .Include(m => m.Flashcards)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (material == null)
                return Result<bool>.Failure($"Material with id '{request.Id}' was not found.");

            if (material.UserId != request.UserId)
                return Result<bool>.Failure("You don't have permission to delete this material.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                _context.LearningMaterials.Remove(material);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database error while deleting material {MaterialId}", request.Id);
                
                if (ex.InnerException?.Message.Contains("REFERENCE") == true)
                    return Result<bool>.Failure("Cannot delete material because it has associated flashcards. Please delete them first.");
                
                throw new SqlException("Failed to delete material", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlException)
        {
            _logger.LogError(ex, "Unexpected error while deleting material {MaterialId}", request.Id);
            return Result<bool>.Failure("An unexpected error occurred while deleting the material.");
        }
    }
}