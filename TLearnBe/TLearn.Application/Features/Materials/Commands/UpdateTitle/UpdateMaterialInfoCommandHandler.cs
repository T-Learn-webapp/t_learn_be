using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Commands.UpdateTitle;

public class UpdateMaterialInfoCommandHandler
    : IRequestHandler<UpdateMaterialInfoCommand, Result<LearningMaterialDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<UpdateMaterialInfoCommandHandler> _logger;

    public UpdateMaterialInfoCommandHandler(
        TLearnDbContext context,
        ILogger<UpdateMaterialInfoCommandHandler> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task<Result<LearningMaterialDto>> Handle(
        UpdateMaterialInfoCommand request,
        CancellationToken cancellationToken)

    {
        try

        {
            if (string.IsNullOrWhiteSpace(request.Title))

            {
                return Result<LearningMaterialDto>
                    .Failure("Tiêu đề tài liệu không được để trống.");
            }

            var material = await _context.LearningMaterials
                .Include(m => m.Subject)
                .FirstOrDefaultAsync(
                    m => m.Id == request.MaterialId,
                    cancellationToken);

            if (material == null)

            {
                return Result<LearningMaterialDto>
                    .Failure("Tài liệu không tồn tại.");
            }

            if (material.UserId != request.UserId &&
                !material.Subject.CanUserEdit(request.UserId))

            {
                return Result<LearningMaterialDto>
                    .Failure("Bạn không có quyền cập nhật tài liệu này.");
            }

            material.Title = request.Title.Trim();

            material.Summary = request.Summary?.Trim();

            material.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var flashcardCount = await _context.Flashcards
                .CountAsync(
                    f => f.MaterialId == material.Id,
                    cancellationToken);

            var dto = new LearningMaterialDto

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
            };

            return Result<LearningMaterialDto>.Success(dto);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi cập nhật thông tin tài liệu {MaterialId}",
                request.MaterialId);

            return Result<LearningMaterialDto>
                .Failure("Đã xảy ra lỗi khi cập nhật thông tin tài liệu.");
        }
    }
}