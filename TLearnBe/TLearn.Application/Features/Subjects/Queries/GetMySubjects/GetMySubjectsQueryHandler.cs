using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Queries.GetMySubjects;

public class GetMySubjectsQueryHandler : IRequestHandler<GetMySubjectsQuery, Result<PagedResult<SubjectDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetMySubjectsQueryHandler> _logger;

    public GetMySubjectsQueryHandler(TLearnDbContext context, ILogger<GetMySubjectsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<SubjectDto>>> Handle(GetMySubjectsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Subjects.Where(s => s.UserId == request.UserId && s.IsDeleted == false);

            if (request.OnlyPublic.HasValue && request.OnlyPublic.Value)
            {
                query = query.Where(s => s.IsPublic);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(searchTerm) || 
                                         (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, request.SortBy, request.IsDescending);

            var subjects = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Color = s.Color,
                    Icon = s.Icon,
                    IsPublic = s.IsPublic,
                    OwnerId = s.UserId,
                    MaterialCount = s.Materials.Count,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<SubjectDto>(subjects, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<SubjectDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting subjects for user {UserId}", request.UserId);
            return Result<PagedResult<SubjectDto>>.Failure("An error occurred while retrieving your subjects.");
        }
    }

    private IQueryable<Subject> ApplySorting(IQueryable<Subject> query, string? sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt);
        }

        var sortExpression = sortBy.ToLower();
        query = sortExpression switch
        {
            "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "materialcount" => isDescending ? query.OrderByDescending(s => s.Materials.Count) : query.OrderBy(s => s.Materials.Count),
            _ => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        return query;
    }
}