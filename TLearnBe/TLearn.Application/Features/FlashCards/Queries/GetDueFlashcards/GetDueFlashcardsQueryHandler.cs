using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.FlashCards.Queries.GetDueFlashcards;

public class GetDueFlashcardsQueryHandler
    : IRequestHandler<GetDueFlashcardsQuery, Result<PagedResult<FlashcardDetailsDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetDueFlashcardsQueryHandler> _logger;

    public GetDueFlashcardsQueryHandler(
        TLearnDbContext context,
        ICurrentUserService currentUser,
        ILogger<GetDueFlashcardsQueryHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FlashcardDetailsDto>>> Handle(
        GetDueFlashcardsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUser.UserId;

            if (!currentUserId.HasValue)
            {
                return Result<PagedResult<FlashcardDetailsDto>>
                    .Failure("Chưa đăng nhập.");
            }

            var now = DateTime.UtcNow;

            var query = _context.Flashcards
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    !x.Material.IsDeleted &&
                    !x.Material.Subject.IsDeleted)
                .Where(x =>
                    x.Material.Subject.UserId == currentUserId.Value ||
                    x.Material.Subject.Members.Any(m =>
                        m.UserId == currentUserId.Value &&
                        !m.IsDeleted));

            if (request.MaterialId.HasValue)
            {
                query = query.Where(x =>
                    x.MaterialId == request.MaterialId.Value);
            }

            if (request.SubjectId.HasValue)
            {
                query = query.Where(x =>
                    x.Material.SubjectId == request.SubjectId.Value);
            }

            query = query.Where(x =>
                !x.UserProgresses.Any(p =>
                    p.UserId == currentUserId.Value) ||
                x.UserProgresses.Any(p =>
                    p.UserId == currentUserId.Value &&
                    p.NextReviewDate <= now));

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.UserProgresses
                    .Where(p => p.UserId == currentUserId.Value)
                    .Select(p => p.NextReviewDate)
                    .FirstOrDefault())
                .ThenByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                .ToListAsync(cancellationToken);

            var result = new PagedResult<FlashcardDetailsDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagedResult<FlashcardDetailsDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách flashcard đến hạn ôn");

            return Result<PagedResult<FlashcardDetailsDto>>
                .Failure("Đã xảy ra lỗi khi lấy danh sách flashcard đến hạn ôn.");
        }
    }
}