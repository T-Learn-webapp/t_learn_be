using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Application.Features.Subjects.Queries.GetMaterialsBySubject;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Materials.Queries.GetMaterialsBySubject;

public class
    GetMaterialsBySubjectQueryHandler : IRequestHandler<GetMaterialsBySubjectQuery,
    Result<PagedResult<LearningMaterialDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetMaterialsBySubjectQueryHandler> _logger;

    public GetMaterialsBySubjectQueryHandler(TLearnDbContext context, ILogger<GetMaterialsBySubjectQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<LearningMaterialDto>>> Handle(
        GetMaterialsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .Include(s => s.Members)
                .FirstOrDefaultAsync(
                    s => s.Id == request.SubjectId && !s.IsDeleted,
                    cancellationToken);

            if (subject == null)
            {
                return Result<PagedResult<LearningMaterialDto>>
                    .Failure("Môn học không tồn tại.");
            }

            var canView = subject.UserId == request.UserId ||
                          subject.Members.Any(m => m.UserId == request.UserId);

            if (!canView)
            {
                return Result<PagedResult<LearningMaterialDto>>
                    .Failure("Bạn không có quyền xem tài liệu trong môn học này.");
            }

            var query = _context.LearningMaterials
                .AsNoTracking()
                .Include(m => m.Subject)
                .Where(m =>
                    m.SubjectId == request.SubjectId &&
                    !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();

                query = query.Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    (m.Summary != null &&
                     m.Summary.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(
                query,
                request.SortBy,
                request.IsDescending);

            var materials = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new LearningMaterialDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    Summary = m.Summary,
                    SubjectId = m.SubjectId,
                    SubjectName = m.Subject.Name,
                    FlashcardCount = m.Flashcards.Count,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<LearningMaterialDto>(
                materials,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagedResult<LearningMaterialDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy danh sách tài liệu của môn học {SubjectId}",
                request.SubjectId);

            return Result<PagedResult<LearningMaterialDto>>
                .Failure("Đã xảy ra lỗi khi lấy danh sách tài liệu.");
        }
    }

    private IQueryable<LearningMaterial> ApplySorting(IQueryable<LearningMaterial> query, string? sortBy,
        bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return isDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt);
        }

        var sortExpression = sortBy.ToLower();
        query = sortExpression switch
        {
            "title" => isDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "updatedat" => isDescending ? query.OrderByDescending(m => m.UpdatedAt) : query.OrderBy(m => m.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
        };

        return query;
    }
}