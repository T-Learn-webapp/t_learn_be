using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateManyFlashCard;

public class UpdateManyFlashcardsCommandHandler
    : IRequestHandler<UpdateManyFlashcardsCommand, Result<List<FlashcardDto>>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IRedisService _redisService;

    private readonly ILogger<UpdateManyFlashcardsCommandHandler> _logger;

    public UpdateManyFlashcardsCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<UpdateManyFlashcardsCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _redisService = redisService;

        _logger = logger;
    }

    public async Task<Result<List<FlashcardDto>>> Handle(
        UpdateManyFlashcardsCommand request,
        CancellationToken cancellationToken)

    {
        try

        {
            if (request.MaterialId == Guid.Empty)

            {
                return Result<List<FlashcardDto>>.Failure("MaterialId không hợp lệ.");
            }

            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)

            {
                return Result<List<FlashcardDto>>.Failure("Chưa đăng nhập.");
            }

            if (request.Flashcards == null || !request.Flashcards.Any())

            {
                return Result<List<FlashcardDto>>.Failure("Danh sách flashcard không được để trống.");
            }

            var duplicatedIds = request.Flashcards
                .GroupBy(x => x.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedIds.Any())

            {
                return Result<List<FlashcardDto>>.Failure("Danh sách có flashcard bị trùng Id.");
            }

            var invalidItems = request.Flashcards
                .Where(x =>
                    x.Id == Guid.Empty ||
                    string.IsNullOrWhiteSpace(x.Front) ||
                    string.IsNullOrWhiteSpace(x.Back))
                .ToList();

            if (invalidItems.Any())

            {
                return Result<List<FlashcardDto>>.Failure(
                    "Có flashcard không hợp lệ. Id, mặt trước và mặt sau không được để trống.");
            }

            var flashcardIds = request.Flashcards
                .Select(x => x.Id)
                .ToList();

            var flashcards = await _context.Flashcards
                .Include(x => x.Material)
                .ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Members)
                .Where(x =>
                    x.MaterialId == request.MaterialId &&
                    flashcardIds.Contains(x.Id) &&
                    !x.IsDeleted)
                .ToListAsync(cancellationToken);
            if (flashcards.Count != flashcardIds.Count)

            {
                return Result<List<FlashcardDto>>.Failure(
                    "Có flashcard không tồn tại hoặc đã bị xoá.");
            }

            if (flashcards.Count != flashcardIds.Count)

            {
                return Result<List<FlashcardDto>>.Failure(
                    "Có flashcard không tồn tại, đã bị xoá hoặc không thuộc tài liệu này.");
            }

            var material = flashcards.First().Material;

            if (!material.Subject.CanUserEdit(currentUserId.Value))

            {
                return Result<List<FlashcardDto>>.Failure(
                    "Bạn không có quyền cập nhật flashcard trong tài liệu này.");
            }

            var requestMap = request.Flashcards
                .ToDictionary(x => x.Id);

            var now = DateTime.UtcNow;

            foreach (var flashcard in flashcards)

            {
                var item = requestMap[flashcard.Id];

                flashcard.Front = item.Front.Trim();

                flashcard.Back = item.Back.Trim();

                flashcard.Hint = string.IsNullOrWhiteSpace(item.Hint)
                    ? null
                    : item.Hint.Trim();

                flashcard.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardCacheAsync(material, flashcards);

            var result = flashcards
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

                    UpdatedAt = x.UpdatedAt
                })
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();

            return Result<List<FlashcardDto>>.Success(result);
        }

        catch (Exception ex)

        {
            _logger.LogError(ex, "Lỗi khi cập nhật nhiều flashcard");

            return Result<List<FlashcardDto>>.Failure(
                "Đã xảy ra lỗi khi cập nhật danh sách flashcard.");
        }
    }

    private async Task RemoveFlashcardCacheAsync(
        LearningMaterial material,
        List<Flashcard> flashcards)

    {
        var subjectUserIds = material.Subject.Members
            .Where(m => !m.IsDeleted)
            .Select(m => m.UserId)
            .ToHashSet();

        subjectUserIds.Add(material.Subject.UserId);

        foreach (var userId in subjectUserIds)

        {
            await _redisService.RemoveAsync(
                CacheKeys.FlashcardsByMaterial(material.Id, userId));

            await _redisService.RemoveAsync(
                CacheKeys.DueFlashcards(userId));

            foreach (var flashcard in flashcards)

            {
                await _redisService.RemoveAsync(
                    CacheKeys.FlashcardDetail(flashcard.Id, userId));
            }
        }

        await _redisService.RemoveAsync(
            CacheKeys.FlashcardCountByMaterial(material.Id));
    }
}