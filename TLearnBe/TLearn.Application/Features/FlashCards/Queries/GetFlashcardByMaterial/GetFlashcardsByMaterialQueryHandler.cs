using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Queries.GetFlashcardByMaterial;

public class GetFlashcardsByMaterialQueryHandler
    : IRequestHandler<GetFlashcardsByMaterialQuery, Result<PagedResult<FlashcardDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<GetFlashcardsByMaterialQueryHandler> _logger;

    public GetFlashcardsByMaterialQueryHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<GetFlashcardsByMaterialQueryHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FlashcardDto>>> Handle(
        GetFlashcardsByMaterialQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<PagedResult<FlashcardDto>>
                    .Failure("Chưa đăng nhập.");
            }

            var cacheKey = CacheKeys.FlashcardsByMaterial(
                request.MaterialId,
                currentUserId.Value);

            var canUseCache =
                string.IsNullOrWhiteSpace(request.SearchTerm) &&
                request.PageNumber == 1 &&
                request.Status == null;

            if (canUseCache)
            {
                var cached = await _redisService.GetAsync(cacheKey);

                if (!string.IsNullOrWhiteSpace(cached))
                {
                    var cachedResult = JsonSerializer.Deserialize<PagedResult<FlashcardDto>>(
                        cached,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (cachedResult != null)
                    {
                        return Result<PagedResult<FlashcardDto>>.Success(cachedResult);
                    }
                }
            }

            var material = await _context.LearningMaterials
                .AsNoTracking()
                .Include(x => x.Subject)
                    .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.MaterialId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (material == null)
            {
                return Result<PagedResult<FlashcardDto>>
                    .Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserView(currentUserId.Value))
            {
                return Result<PagedResult<FlashcardDto>>
                    .Failure("Bạn không có quyền xem flashcard của tài liệu này.");
            }

            var query = _context.Flashcards
                .AsNoTracking()
                .Where(x =>
                    x.MaterialId == request.MaterialId &&
                    !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();

                query = query.Where(x =>
                    x.Front.ToLower().Contains(searchTerm) ||
                    x.Back.ToLower().Contains(searchTerm) ||
                    (x.Hint != null && x.Hint.ToLower().Contains(searchTerm)));
            }

            var now = DateTime.UtcNow;

            if (request.Status.HasValue)
            {
                query = request.Status.Value switch
                {
                    FlashcardLearningStatus.NotStudied => query.Where(x =>
                        !x.UserProgresses.Any(p => p.UserId == currentUserId.Value)),

                    FlashcardLearningStatus.Studied => query.Where(x =>
                        x.UserProgresses.Any(p =>
                            p.UserId == currentUserId.Value &&
                            p.LastReviewedAt != null)),

                    FlashcardLearningStatus.NeedReview => query.Where(x =>
                        x.UserProgresses.Any(p =>
                            p.LastQuality == FlashcardReviewQuality.Again &&
                            p.UserId == currentUserId.Value 
                            // &&
                            // p.NextReviewDate != null &&
                            // p.NextReviewDate <= now
                            )
                        )
                    ,

                    FlashcardLearningStatus.Remembered => query.Where(x =>
                        x.UserProgresses.Any(p =>
                            p.UserId == currentUserId.Value &&
                            p.LastQuality == FlashcardReviewQuality.Good &&
                            p.NextReviewDate != null &&
                            p.NextReviewDate > now)),

                    _ => query
                };
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new FlashcardDto
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
                        .FirstOrDefault(),

                    LearningStatus = !x.UserProgresses.Any(p => p.UserId == currentUserId.Value)
                        ? FlashcardLearningStatus.NotStudied
                        : x.UserProgresses.Any(p =>
                            p.UserId == currentUserId.Value &&
                            p.NextReviewDate != null &&
                            p.NextReviewDate <= now)
                            ? FlashcardLearningStatus.NeedReview
                            : x.UserProgresses.Any(p =>
                                p.UserId == currentUserId.Value &&
                                p.LastQuality == FlashcardReviewQuality.Good)
                                ? FlashcardLearningStatus.Remembered
                                : FlashcardLearningStatus.Studied
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<FlashcardDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            if (canUseCache)
            {
                var json = JsonSerializer.Serialize(result);

                await _redisService.SetAsync(
                    cacheKey,
                    json,
                    TimeSpan.FromMinutes(5));
            }

            return Result<PagedResult<FlashcardDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy danh sách flashcard của material {MaterialId}",
                request.MaterialId);

            return Result<PagedResult<FlashcardDto>>
                .Failure("Đã xảy ra lỗi khi lấy danh sách flashcard.");
        }
    }
}
