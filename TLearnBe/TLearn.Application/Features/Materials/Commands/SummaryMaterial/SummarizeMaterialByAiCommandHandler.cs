using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Materials.Commands.SummaryMaterial;

public class SummarizeMaterialByAiCommandHandler
    : IRequestHandler<SummarizeMaterialByAiCommand, Result<LearningMaterialDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IAiService _flashcardAiService;

    private readonly ILogger<SummarizeMaterialByAiCommandHandler> _logger;

    public SummarizeMaterialByAiCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IAiService flashcardAiService,
        ILogger<SummarizeMaterialByAiCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _flashcardAiService = flashcardAiService;

        _logger = logger;
    }

    public async Task<Result<LearningMaterialDto>> Handle(
        SummarizeMaterialByAiCommand request,
        CancellationToken cancellationToken)

    {
        try

        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<LearningMaterialDto>.Failure("Chưa đăng nhập.");
            }

            var material = await _context.LearningMaterials
                .Include(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.MaterialId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (material == null)

            {
                return Result<LearningMaterialDto>.Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserEdit(currentUserId.Value))

            {
                return Result<LearningMaterialDto>.Failure(
                    "Bạn không có quyền cập nhật tóm tắt cho tài liệu này.");
            }

            if (string.IsNullOrWhiteSpace(material.Content))

            {
                return Result<LearningMaterialDto>.Failure(
                    "Tài liệu chưa có nội dung để tóm tắt bằng AI.");
            }

            var sourceContent = material.Content.Trim();

            var summarizedContent = await SummarizeContentSafelyAsync(
                sourceContent,
                material.Id,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(summarizedContent))

            {
                return Result<LearningMaterialDto>.Failure(
                    "AI không tạo được nội dung tóm tắt hợp lệ.");
            }

            material.Summary = summarizedContent.Trim();

            material.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var flashcardCount = await _context.Flashcards
                .CountAsync(
                    x => x.MaterialId == material.Id &&
                         !x.IsDeleted,
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

        catch (InvalidOperationException ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi AI tóm tắt material {MaterialId}",
                request.MaterialId);

            return Result<LearningMaterialDto>.Failure(ex.Message);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi không xác định khi AI tóm tắt material {MaterialId}",
                request.MaterialId);

            return Result<LearningMaterialDto>.Failure(
                $"Đã xảy ra lỗi khi tóm tắt tài liệu bằng AI: {ex.Message}");
        }
    }

    private async Task<string> SummarizeContentSafelyAsync(
        string sourceContent,
        Guid materialId,
        CancellationToken cancellationToken)

    {
        try

        {
            if (sourceContent.Length > 20000)

            {
                sourceContent = sourceContent[..20000];
            }

            var summarizedContent = await _flashcardAiService.SummarizeContentAsync(
                sourceContent,
                cancellationToken);

            return summarizedContent;
        }

        catch (TaskCanceledException ex)

        {
            _logger.LogError(
                ex,
                "Timeout khi gọi AI tóm tắt material {MaterialId}",
                materialId);

            throw new InvalidOperationException(
                "AI tóm tắt nội dung phản hồi quá lâu. Vui lòng thử lại sau.",
                ex);
        }

        catch (HttpRequestException ex)

        {
            _logger.LogError(
                ex,
                "Không thể kết nối AI service khi tóm tắt material {MaterialId}",
                materialId);

            throw new InvalidOperationException(
                "Không thể kết nối tới Ollama để tóm tắt nội dung. Hãy kiểm tra Ollama đã chạy chưa.",
                ex);
        }
    }
}