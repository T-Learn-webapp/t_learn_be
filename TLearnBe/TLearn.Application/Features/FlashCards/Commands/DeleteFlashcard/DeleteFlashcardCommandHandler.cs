using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.DeleteFlashcard;

public class DeleteFlashcardCommandHandler
    : IRequestHandler<DeleteFlashcardCommand, Result<bool>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<DeleteFlashcardCommandHandler> _logger;

    public DeleteFlashcardCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<DeleteFlashcardCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteFlashcardCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<bool>.Failure("Chưa đăng nhập.");
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
                return Result<bool>.Failure("Flashcard không tồn tại.");
            }

            if (!flashcard.Material.Subject.CanUserEdit(currentUserId.Value))
            {
                return Result<bool>.Failure("Bạn không có quyền xoá flashcard này.");
            }

            flashcard.IsDeleted = true;
            flashcard.DeletedAt = DateTime.UtcNow;
            flashcard.DeletedByUserId = currentUserId.Value;
            flashcard.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardCacheAsync(flashcard);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi xoá flashcard {FlashcardId}",
                request.FlashcardId);

            return Result<bool>.Failure("Đã xảy ra lỗi khi xoá flashcard.");
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