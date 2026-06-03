using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.GenerateFlashcardsByAi;

public class GenerateFlashcardsByAiCommandHandler
    : IRequestHandler<GenerateFlashcardsByAiCommand, Result<List<AiGeneratedFlashcardDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IAiService _flashcardAiService;
    
    private readonly IAiUsageLimiter _aiUsageLimiter;

    private readonly ILogger<GenerateFlashcardsByAiCommandHandler> _logger;

    public GenerateFlashcardsByAiCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IAiService flashcardAiService,
        IAiUsageLimiter aiUsageLimiter,
        ILogger<GenerateFlashcardsByAiCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _flashcardAiService = flashcardAiService;
        
        _aiUsageLimiter = aiUsageLimiter;

        _logger = logger;
    }

    public async Task<Result<List<AiGeneratedFlashcardDto>>> Handle(
        GenerateFlashcardsByAiCommand request,
        CancellationToken cancellationToken)

    {
        try

        {
           
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure("Chưa đăng nhập.");
            }
            
            var usageCheck = await _aiUsageLimiter.CheckAsync(
                currentUserId.Value,
                cancellationToken);

            if (!usageCheck.IsAllowed)
            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    usageCheck.ErrorMessage ?? "Bạn đã vượt quá giới hạn sử dụng AI.");
            }

            if (request.Count <= 0 || request.Count > 20)

            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    "Số lượng flashcard phải nằm trong khoảng 1 đến 20.");
            }

            var material = await _context.LearningMaterials
                .Include(x => x.Subject)
                    .ThenInclude(x => x.Members)
                .Include(x => x.Flashcards)
                .FirstOrDefaultAsync(
                    x => x.Id == request.MaterialId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (material == null)

            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserEdit(currentUserId.Value))

            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    "Bạn không có quyền tạo flashcard cho tài liệu này.");
            }

            var sourceContent = material.Content;

            if (string.IsNullOrWhiteSpace(sourceContent))

            {
                sourceContent = material.Summary;
            }

            if (string.IsNullOrWhiteSpace(sourceContent))

            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    "Tài liệu chưa có nội dung để tạo flashcard bằng AI.");
            }

            if (sourceContent.Length > 8000)
            {
                sourceContent = await SummarizeLongContentAsync(
                    sourceContent,
                    request.MaterialId,
                    cancellationToken);
            }

            var contentChunks = string.IsNullOrWhiteSpace(sourceContent)
                ? new List<string>()
                : new List<string> { sourceContent };

            if (!contentChunks.Any())
            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    "Tài liệu chưa có nội dung phù hợp để tạo flashcard bằng AI.");
            }

            var excludedFronts = material.Flashcards
                .Where(x => !x.IsDeleted)
                .Select(x => x.Front.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var aiFlashcards = new List<AiGeneratedFlashcardDto>();
            const int maxAttempts = 3;

            for (var chunkIndex = 0; chunkIndex < contentChunks.Count && aiFlashcards.Count < request.Count; chunkIndex++)
            {
                var countForChunk = CalculateCountForChunk(
                    request.Count,
                    chunkIndex,
                    contentChunks.Count,
                    aiFlashcards.Count);

                await GenerateFlashcardsFromChunkAsync(
                    contentChunks[chunkIndex],
                    countForChunk,
                    request,
                    excludedFronts,
                    aiFlashcards,
                    cancellationToken);
            }

            for (var attempt = 1; attempt <= maxAttempts && aiFlashcards.Count < request.Count; attempt++)
            {
                var remainingCount = request.Count - aiFlashcards.Count;
                var fallbackChunk = contentChunks[(attempt - 1) % contentChunks.Count];

                await GenerateFlashcardsFromChunkAsync(
                    fallbackChunk,
                    remainingCount,
                    request,
                    excludedFronts,
                    aiFlashcards,
                    cancellationToken);
            }

            if (!aiFlashcards.Any())
            {
                return Result<List<AiGeneratedFlashcardDto>>.Failure(
                    "AI không tạo được flashcard hợp lệ.");
            }
            await _aiUsageLimiter.RecordAsync(
                currentUserId.Value,
                "GenerateFlashcards",
                cancellationToken);
            
            
            return Result<List<AiGeneratedFlashcardDto>>.Success(
                aiFlashcards.Take(request.Count).ToList());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi tạo flashcard bằng AI cho material {MaterialId}",
                request.MaterialId);

            return Result<List<AiGeneratedFlashcardDto>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi tạo flashcard bằng AI cho material {MaterialId}",
                request.MaterialId);

            return Result<List<AiGeneratedFlashcardDto>>.Failure(
                $"Đã xảy ra lỗi khi tạo flashcard bằng AI: {ex.Message}");
        }
    }

    private async Task<string> SummarizeLongContentAsync(
        string sourceContent,
        Guid materialId,
        CancellationToken cancellationToken)
    {
        try
        {
            var chunks = PickRepresentativeChunks(
                SplitContentIntoChunks(sourceContent, 6000),
                maxChunks: 5);

            if (!chunks.Any())
            {
                return sourceContent.Length > 8000
                    ? sourceContent[..8000]
                    : sourceContent;
            }

            
            var contentForSummary = string.Join(
                "\n\n--- PHẦN TIẾP THEO ---\n\n",
                chunks);

            var summarizedContent = await _flashcardAiService.SummarizeContentAsync(
                contentForSummary,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(summarizedContent))
            {
                return sourceContent.Length > 8000
                    ? sourceContent[..8000]
                    : sourceContent;
            }

            return summarizedContent;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout khi gọi AI tóm tắt nội dung cho material {MaterialId}",
                materialId);

            throw new InvalidOperationException(
                "AI tóm tắt nội dung phản hồi quá lâu. Vui lòng thử lại sau.",
                ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Không thể kết nối AI service khi tóm tắt nội dung cho material {MaterialId}",
                materialId);

            throw new InvalidOperationException(
                "Không thể kết nối tới Ollama để tóm tắt nội dung. Hãy kiểm tra Ollama đã chạy chưa.",
                ex);
        }
    }

    private async Task GenerateFlashcardsFromChunkAsync(
        string chunkContent,
        int count,
        GenerateFlashcardsByAiCommand request,
        List<string> excludedFronts,
        List<AiGeneratedFlashcardDto> aiFlashcards,
        CancellationToken cancellationToken)
    {
        if (count <= 0 || aiFlashcards.Count >= request.Count)
        {
            return;
        }

        List<AiGeneratedFlashcard>? generatedFlashcards;

        try
        {
            generatedFlashcards =  await  _flashcardAiService.GenerateFlashcardsAsync(
                chunkContent,
                count,
                excludedFronts.Concat(aiFlashcards.Select(x => x.Front)).ToList(),
                cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout khi gọi AI tạo flashcard cho material {MaterialId}",
                request.MaterialId);

            throw new InvalidOperationException(
                "AI phản hồi quá lâu. Vui lòng thử giảm số lượng flashcard hoặc thử lại sau.",
                ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Không thể kết nối AI service khi tạo flashcard cho material {MaterialId}",
                request.MaterialId);

            throw new InvalidOperationException(
                "Không thể kết nối tới Ollama. Hãy kiểm tra Ollama đã chạy chưa.",
                ex);
        }

        if (generatedFlashcards == null || !generatedFlashcards.Any())
        {
            return;
        }

        foreach (var generated in generatedFlashcards)
        {
            var front = (generated.Front ?? string.Empty).Trim();
            var back = (generated.Back ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back))
            {
                continue;
            }

            var isDuplicate =
                excludedFronts.Any(x => x.Equals(front, StringComparison.OrdinalIgnoreCase)) ||
                aiFlashcards.Any(x =>
                    x.Front.Equals(front, StringComparison.OrdinalIgnoreCase) ||
                    x.Back.Equals(back, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                continue;
            }

            aiFlashcards.Add(new AiGeneratedFlashcardDto
            {
                Front = front,
                Back = back,
                Hint = string.IsNullOrWhiteSpace(generated.Hint)
                    ? null
                    : generated.Hint.Trim()
            });

            if (aiFlashcards.Count >= request.Count)
            {
                break;
            }
        }
    }

    private static List<string> SplitContentIntoChunks(
        string content,
        int maxChunkLength)
    {
        var chunks = new List<string>();

        var paragraphs = content
            .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (!paragraphs.Any())
        {
            paragraphs = content
                .Split(new[] { ". ", "。", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > maxChunkLength)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }

                for (var start = 0; start < paragraph.Length; start += maxChunkLength)
                {
                    var length = Math.Min(maxChunkLength, paragraph.Length - start);
                    chunks.Add(paragraph.Substring(start, length).Trim());
                }

                continue;
            }

            if (currentChunk.Length + paragraph.Length + 2 > maxChunkLength)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            currentChunk.AppendLine(paragraph);
            currentChunk.AppendLine();
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static List<string> PickRepresentativeChunks(
        List<string> chunks,
        int maxChunks)
    {
        if (chunks.Count <= maxChunks)
        {
            return chunks;
        }

        var result = new List<string>();
        var usedIndexes = new HashSet<int>();
        var step = (double)(chunks.Count - 1) / (maxChunks - 1);

        for (var i = 0; i < maxChunks; i++)
        {
            var index = (int)Math.Round(i * step);

            if (usedIndexes.Add(index))
            {
                result.Add(chunks[index]);
            }
        }

        return result;
    }

    private static int CalculateCountForChunk(
        int totalCount,
        int chunkIndex,
        int totalChunks,
        int alreadyGenerated)
    {
        var remainingChunks = totalChunks - chunkIndex;
        var remainingCount = totalCount - alreadyGenerated;

        if (remainingChunks <= 0 || remainingCount <= 0)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling((double)remainingCount / remainingChunks));
    }
}