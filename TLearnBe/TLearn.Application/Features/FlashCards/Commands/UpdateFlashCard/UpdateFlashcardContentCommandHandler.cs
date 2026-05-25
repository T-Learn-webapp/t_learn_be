using TLearn.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateFlashCard;

public class UpdateFlashcardContentCommandHandler
    : IRequestHandler<UpdateFlashcardContentCommand, Result<FlashcardDto>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<UpdateFlashcardContentCommandHandler> _logger;

    public UpdateFlashcardContentCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<UpdateFlashcardContentCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<FlashcardDto>> Handle(
        UpdateFlashcardContentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<FlashcardDto>.Failure("Chưa đăng nhập.");
            }

            if (string.IsNullOrWhiteSpace(request.Front))
            {
                return Result<FlashcardDto>.Failure("Mặt trước flashcard không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(request.Back))
            {
                return Result<FlashcardDto>.Failure("Mặt sau flashcard không được để trống.");
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
                return Result<FlashcardDto>.Failure("Flashcard không tồn tại.");
            }

            if (!flashcard.Material.Subject.CanUserEdit(currentUserId.Value))
            {
                return Result<FlashcardDto>.Failure("Bạn không có quyền cập nhật flashcard này.");
            }

            flashcard.Front = request.Front.Trim();
            flashcard.Back = request.Back.Trim();
            flashcard.Hint = string.IsNullOrWhiteSpace(request.Hint)
                ? null
                : request.Hint.Trim();
            flashcard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await RemoveFlashcardCacheAsync(flashcard);

            var dto = new FlashcardDto
            {
                Id = flashcard.Id,
                Front = flashcard.Front,
                Back = flashcard.Back,
                Hint = flashcard.Hint,
                IsAIGenerated = flashcard.IsAIGenerated,
                MaterialId = flashcard.MaterialId,
                CreatedByUserId = flashcard.CreatedByUserId,
                CreatedAt = flashcard.CreatedAt,
                UpdatedAt = flashcard.UpdatedAt
            };

            return Result<FlashcardDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi cập nhật nội dung flashcard {FlashcardId}",
                request.FlashcardId);

            return Result<FlashcardDto>.Failure(
                "Đã xảy ra lỗi khi cập nhật flashcard.");
        }
    }

    private async Task RemoveFlashcardCacheAsync(Flashcard flashcard)
    {
        var subjectUserIds = flashcard.Material.Subject.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .ToHashSet();

        subjectUserIds.Add(flashcard.Material.Subject.UserId);

        foreach (var userId in subjectUserIds)
        {
            await _redisService.RemoveAsync(
                CacheKeys.FlashcardsByMaterial(flashcard.MaterialId, userId));

            await _redisService.RemoveAsync(
                CacheKeys.FlashcardDetail(flashcard.Id, userId));

            await _redisService.RemoveAsync(
                CacheKeys.DueFlashcards(userId));
        }

        await _redisService.RemoveAsync(
            CacheKeys.FlashcardCountByMaterial(flashcard.MaterialId));
    }
}