using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.ResetProgress;

public class ResetFlashcardProgressCommandHandler
    : IRequestHandler<ResetFlashcardProgressCommand, Result<bool>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IRedisService _redisService;

    private readonly ILogger<ResetFlashcardProgressCommandHandler> _logger;

    public ResetFlashcardProgressCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<ResetFlashcardProgressCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _redisService = redisService;

        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ResetFlashcardProgressCommand request,
        CancellationToken cancellationToken)

    {
        try

        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<bool>.Failure("Chưa đăng nhập.");
            }

            if (request.MaterialId == Guid.Empty)

            {
                return Result<bool>.Failure("MaterialId không hợp lệ.");
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
                return Result<bool>.Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserView(currentUserId.Value))

            {
                return Result<bool>.Failure(
                    "Bạn không có quyền học flashcard trong tài liệu này.");
            }

            var progresses = await _context.UserFlashcardProgresses
                .Include(x => x.Flashcard)
                .Where(x =>
                    x.UserId == currentUserId.Value &&
                    x.Flashcard.MaterialId == request.MaterialId &&
                    !x.Flashcard.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!progresses.Any())

            {
                await RemoveFlashcardProgressCacheAsync(
                    request.MaterialId,
                    currentUserId.Value);

                return Result<bool>.Success(true);
            }

            var now = DateTime.UtcNow;

            foreach (var progress in progresses)

            {
                progress.EaseFactor = 2.5;

                progress.Interval = 1;

                progress.RepetitionCount = 0;

                progress.NextReviewDate = null;

                progress.LastReviewedAt = null;

                progress.LastQuality = null;

                progress.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardProgressCacheAsync(
                request.MaterialId,
                currentUserId.Value);

            return Result<bool>.Success(true);
        }

        catch (Exception ex)

        {
            _logger.LogError(
                ex,
                "Lỗi khi reset tiến độ flashcard của material {MaterialId}",
                request.MaterialId);

            return Result<bool>.Failure(
                "Đã xảy ra lỗi khi reset tiến độ flashcard.");
        }
    }

    private async Task RemoveFlashcardProgressCacheAsync(
        Guid materialId,
        Guid userId)

    {
        await _redisService.RemoveAsync(
            CacheKeys.FlashcardsByMaterial(materialId, userId));

        await _redisService.RemoveAsync(
            CacheKeys.DueFlashcards(userId));
    }
}