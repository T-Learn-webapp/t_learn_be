using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjects;

public class GetSubjectsQueryHandler 
    : IRequestHandler<GetSubjectsQuery, Result<PagedResult<SubjectDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetSubjectsQueryHandler> _logger;

    public GetSubjectsQueryHandler(
        TLearnDbContext context,
        ILogger<GetSubjectsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<SubjectDto>>> Handle(
        GetSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!request.UserId.HasValue)
            {
                return Result<PagedResult<SubjectDto>>
                    .Failure("Chưa đăng nhập.");
            }

            var userId = request.UserId.Value;

            var query = _context.Subjects
                .AsNoTracking()
                .Include(s => s.Members)
                .Where(s => !s.IsDeleted)
                .Where(s =>
                    s.UserId == userId ||
                    s.Members.Any(m => m.UserId == userId))
                .AsQueryable();

            query = request.Filter switch
            {
                SubjectFilterType.Owned => query.Where(s =>
                    s.UserId == userId),

                SubjectFilterType.Joined => query.Where(s =>
                    s.UserId != userId &&
                    s.Members.Any(m => m.UserId == userId)),

                _ => query
            };

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();

                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchTerm) ||
                    (s.Description != null &&
                     s.Description.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(
                query,
                request.SortBy,
                request.IsDescending);

            var subjects = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    OwnerId = s.UserId,
                    MaterialCount = s.Materials.Count(m => !m.IsDeleted),
                    CreatedAt = s.CreatedAt,
    
                    IsOwner = s.UserId == userId,
                    OwnerName = s.User.FullName,
                    OwnerEmail =  s.User.Email ?? "none",
                    IsMember = s.Members.Any(m => m.UserId == userId),

                    MyPermission = s.UserId == userId
                        ? SubjectPermission.Manage
                        : s.Members
                            .Where(m => m.UserId == userId)
                            .Select(m => (SubjectPermission?)m.Permission)
                            .FirstOrDefault(),

                    Role = s.UserId == userId
                        ? "Owner"
                        : "Member"
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<SubjectDto>(
                subjects,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PagedResult<SubjectDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách môn học");

            return Result<PagedResult<SubjectDto>>
                .Failure("Đã xảy ra lỗi khi lấy danh sách môn học.");
        }
    }

    private IQueryable<Subject> ApplySorting(
        IQueryable<Subject> query,
        string? sortBy,
        bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return isDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt);
        }

        var sortExpression = sortBy.ToLower();

        query = sortExpression switch
        {
            "name" => isDescending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),

            "materialcount" => isDescending
                ? query.OrderByDescending(s => s.Materials.Count(m => !m.IsDeleted))
                : query.OrderBy(s => s.Materials.Count(m => !m.IsDeleted)),

            _ => isDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt)
        };

        return query;
    }
}
