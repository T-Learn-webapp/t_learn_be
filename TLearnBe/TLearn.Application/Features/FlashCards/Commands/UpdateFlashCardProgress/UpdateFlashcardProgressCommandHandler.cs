using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateFlashCardProgress;

public class UpdateFlashcardProgressCommandHandler
    : IRequestHandler<UpdateFlashcardProgressCommand, Result<UserFlashcardProgressDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<UpdateFlashcardProgressCommandHandler> _logger;

    public UpdateFlashcardProgressCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<UpdateFlashcardProgressCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<UserFlashcardProgressDto>> Handle(
        UpdateFlashcardProgressCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<UserFlashcardProgressDto>.Failure("Chưa đăng nhập.");
            }

            var flashcard = await _context.Flashcards
                .Include(x => x.Material)
                .ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.FlashcardId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (flashcard == null)
            {
                return Result<UserFlashcardProgressDto>.Failure("Flashcard không tồn tại.");
            }

            if (!flashcard.Material.Subject.CanUserView(currentUserId.Value))
            {
                return Result<UserFlashcardProgressDto>.Failure("Bạn không có quyền học flashcard này.");
            }
            

            var progress = await _context.UserFlashcardProgresses
                .FirstOrDefaultAsync(
                    x => x.FlashcardId == request.FlashcardId &&
                         x.UserId == currentUserId.Value,
                    cancellationToken);

            if (progress == null)
            {
                progress = new UserFlashcardProgress
                {
                    FlashcardId = request.FlashcardId,
                    UserId = currentUserId.Value,
                    EaseFactor = 2.5,
                    Interval = 1,
                    RepetitionCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserFlashcardProgresses.Add(progress);
            }

            ApplyReviewResult(progress, request.Quality);

            progress.LastQuality = request.Quality;
            progress.LastReviewedAt = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardProgressCacheAsync(
                flashcard.MaterialId,
                flashcard.Id,
                currentUserId.Value);

            var dto = new UserFlashcardProgressDto
            {
                FlashcardId = progress.FlashcardId,
                UserId = progress.UserId,
                EaseFactor = progress.EaseFactor,
                Interval = progress.Interval,
                RepetitionCount = progress.RepetitionCount,
                NextReviewDate = progress.NextReviewDate,
                LastReviewedAt = progress.LastReviewedAt,
                LastQuality = progress.LastQuality
            };

            return Result<UserFlashcardProgressDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi cập nhật tiến độ flashcard {FlashcardId}",
                request.FlashcardId);

            return Result<UserFlashcardProgressDto>.Failure(
                "Đã xảy ra lỗi khi cập nhật tiến độ flashcard.");
        }
    }

    private static void ApplyReviewResult(
        UserFlashcardProgress progress,
        FlashcardReviewQuality quality)
    {
        switch (quality)
        {
            case FlashcardReviewQuality.Again:
                progress.RepetitionCount = 0;
                progress.Interval = 1;
                progress.EaseFactor = Math.Max(1.3, progress.EaseFactor - 0.2);
                progress.NextReviewDate = DateTime.UtcNow.AddDays(1);
                break;

            case FlashcardReviewQuality.Good:
                progress.RepetitionCount += 1;

                if (progress.RepetitionCount == 1)
                {
                    progress.Interval = 1;
                }
                else if (progress.RepetitionCount == 2)
                {
                    progress.Interval = 3;
                }
                else
                {
                    progress.Interval = (int)Math.Round(progress.Interval * progress.EaseFactor);
                }

                progress.EaseFactor = Math.Min(3.0, progress.EaseFactor + 0.05);
                progress.NextReviewDate = DateTime.UtcNow.AddDays(progress.Interval);
                break;
        }
    }
    private async Task RemoveFlashcardProgressCacheAsync(
        Guid materialId,
        Guid flashcardId,
        Guid userId)
    {
        await _redisService.RemoveAsync(
            CacheKeys.FlashcardsByMaterial(materialId, userId));

        await _redisService.RemoveAsync(
            CacheKeys.FlashcardDetail(flashcardId, userId));

        await _redisService.RemoveAsync(
            CacheKeys.DueFlashcards(userId));
    }
}