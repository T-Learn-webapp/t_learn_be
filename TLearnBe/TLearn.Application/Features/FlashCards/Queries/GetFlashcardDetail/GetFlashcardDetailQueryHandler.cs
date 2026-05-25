using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Queries.GetFlashcardDetail;

public class GetFlashcardDetailQueryHandler
    : IRequestHandler<GetFlashcardDetailQuery, Result<FlashcardDetailsDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<GetFlashcardDetailQueryHandler> _logger;

    public GetFlashcardDetailQueryHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<GetFlashcardDetailQueryHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<FlashcardDetailsDto>> Handle(
        GetFlashcardDetailQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<FlashcardDetailsDto>.Failure("Chưa đăng nhập.");
            }

            var cacheKey = CacheKeys.FlashcardDetail(
                request.FlashcardId,
                currentUserId.Value);

            var cached = await _redisService.GetAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(cached))
            {
                var cachedDto = JsonSerializer.Deserialize<FlashcardDetailsDto>(
                    cached,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (cachedDto != null)
                {
                    return Result<FlashcardDetailsDto>.Success(cachedDto);
                }
            }

            var flashcard = await _context.Flashcards
                .AsNoTracking()
                .Where(x =>
                    x.Id == request.FlashcardId &&
                    !x.IsDeleted)
                .Select(x => new
                {
                    Flashcard = x,
                    Subject = x.Material.Subject,
                    Members = x.Material.Subject.Members
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (flashcard == null)
            {
                return Result<FlashcardDetailsDto>.Failure("Flashcard không tồn tại.");
            }

            if (!flashcard.Subject.CanUserView(currentUserId.Value))
            {
                return Result<FlashcardDetailsDto>.Failure("Bạn không có quyền xem flashcard này.");
            }

            var dto = await _context.Flashcards
                .AsNoTracking()
                .Where(x =>
                    x.Id == request.FlashcardId &&
                    !x.IsDeleted)
                .Select(x => new FlashcardDetailsDto
                {
                    Id = x.Id,
                    Front = x.Front,
                    Back = x.Back,
                    Hint = x.Hint,
                    IsAIGenerated = x.IsAIGenerated,
                    MaterialId = x.MaterialId,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,

                    Progress = x.UserProgresses
                        .Where(p => p.UserId == currentUserId.Value)
                        .Select(p => new UserFlashcardProgressDto
                        {
                            FlashcardId = p.FlashcardId,
                            UserId = p.UserId,
                            EaseFactor = p.EaseFactor,
                            Interval = p.Interval,
                            RepetitionCount = p.RepetitionCount,
                            NextReviewDate = p.NextReviewDate,
                            LastReviewedAt = p.LastReviewedAt,
                            LastQuality = p.LastQuality
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
            {
                return Result<FlashcardDetailsDto>.Failure("Flashcard không tồn tại.");
            }

            await _redisService.SetAsync(
                cacheKey,
                JsonSerializer.Serialize(dto),
                TimeSpan.FromMinutes(5));

            return Result<FlashcardDetailsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy chi tiết flashcard {FlashcardId}",
                request.FlashcardId);

            return Result<FlashcardDetailsDto>.Failure(
                "Đã xảy ra lỗi khi lấy chi tiết flashcard.");
        }
    }
}