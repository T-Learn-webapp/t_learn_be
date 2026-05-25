using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.CreateManyFlashcard;

public class CreateManyFlashcardsCommandHandler
    : IRequestHandler<CreateManyFlashcardsCommand, Result<List<FlashcardDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRedisService _redisService;
    private readonly ILogger<CreateManyFlashcardsCommandHandler> _logger;

    public CreateManyFlashcardsCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<CreateManyFlashcardsCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<Result<List<FlashcardDto>>> Handle(
        CreateManyFlashcardsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<List<FlashcardDto>>.Failure("Chưa đăng nhập.");
            }

            if (request.Flashcards == null || !request.Flashcards.Any())
            {
                return Result<List<FlashcardDto>>.Failure("Danh sách flashcard không được để trống.");
            }

            var validFlashcards = request.Flashcards
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Front) &&
                    !string.IsNullOrWhiteSpace(x.Back))
                .ToList();

            if (!validFlashcards.Any())
            {
                return Result<List<FlashcardDto>>.Failure(
                    "Không có flashcard hợp lệ. Mặt trước và mặt sau không được để trống.");
            }

            if (validFlashcards.Count != request.Flashcards.Count)
            {
                return Result<List<FlashcardDto>>.Failure(
                    "Có flashcard thiếu mặt trước hoặc mặt sau.");
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
                return Result<List<FlashcardDto>>.Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserEdit(currentUserId.Value))
            {
                return Result<List<FlashcardDto>>.Failure(
                    "Bạn không có quyền tạo flashcard cho tài liệu này.");
            }

            var now = DateTime.UtcNow;

            var flashcards = validFlashcards
                .Select(x => new Flashcard
                {
                    MaterialId = request.MaterialId,
                    Front = x.Front.Trim(),
                    Back = x.Back.Trim(),
                    Hint = string.IsNullOrWhiteSpace(x.Hint)
                        ? null
                        : x.Hint.Trim(),
                    IsAIGenerated = x.IsAIGenerated,
                    CreatedByUserId = currentUserId.Value,
                    CreatedAt = now
                })
                .ToList();

            _context.Flashcards.AddRange(flashcards);

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardCacheAsync(material);

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
                .ToList();

            return Result<List<FlashcardDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi tạo nhiều flashcard cho material {MaterialId}",
                request.MaterialId);

            return Result<List<FlashcardDto>>.Failure(
                "Đã xảy ra lỗi khi tạo danh sách flashcard.");
        }
    }

    private async Task RemoveFlashcardCacheAsync(LearningMaterial material)
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
        }

        await _redisService.RemoveAsync(
            CacheKeys.FlashcardCountByMaterial(material.Id));
    }
}