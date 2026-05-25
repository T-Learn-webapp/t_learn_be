using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Commands.CreateFlashCard;

public class CreateFlashcardCommandHandler
    : IRequestHandler<CreateFlashcardCommand, Result<FlashcardDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ICurrentUserService _currentUser;

    private readonly IRedisService _redisService;

    private readonly ILogger<CreateFlashcardCommandHandler> _logger;

    public CreateFlashcardCommandHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        IRedisService redisService,
        ILogger<CreateFlashcardCommandHandler> logger)

    {
        _context = context;

        _currentUser = currentUser;

        _redisService = redisService;

        _logger = logger;
    }

    public async Task<Result<FlashcardDto>> Handle(
        CreateFlashcardCommand request,
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

            var material = await _context.LearningMaterials
                .Include(x => x.Subject)
                .ThenInclude(x => x.Members)
                .FirstOrDefaultAsync(
                    x => x.Id == request.MaterialId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (material == null)

            {
                return Result<FlashcardDto>.Failure("Tài liệu không tồn tại.");
            }

            if (!material.Subject.CanUserEdit(currentUserId.Value))

            {
                return Result<FlashcardDto>.Failure("Bạn không có quyền tạo flashcard cho tài liệu này.");
            }

            var flashcard = new Flashcard

            {
                MaterialId = request.MaterialId,

                Front = request.Front.Trim(),

                Back = request.Back.Trim(),

                Hint = string.IsNullOrWhiteSpace(request.Hint)
                    ? null
                    : request.Hint.Trim(),

                IsAIGenerated = request.IsAIGenerated,

                CreatedByUserId = currentUserId.Value,

                CreatedAt = DateTime.UtcNow
            };

            _context.Flashcards.Add(flashcard);

            await _context.SaveChangesAsync(cancellationToken);

            await RemoveFlashcardCacheAsync(
                request.MaterialId,
                currentUserId.Value);

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
                "Lỗi khi tạo flashcard cho material {MaterialId}",
                request.MaterialId);

            return Result<FlashcardDto>.Failure(
                "Đã xảy ra lỗi khi tạo flashcard.");
        }
    }

    private async Task RemoveFlashcardCacheAsync(
        Guid materialId,
        Guid userId)
    {
        await _redisService.RemoveAsync(
            CacheKeys.FlashcardsByMaterial(materialId, userId));
        await _redisService.RemoveAsync(
            CacheKeys.FlashcardCountByMaterial(materialId));
    }
}