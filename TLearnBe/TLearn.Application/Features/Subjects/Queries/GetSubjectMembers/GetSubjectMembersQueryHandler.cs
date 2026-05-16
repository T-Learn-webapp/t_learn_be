using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjectMembers;

public class GetSubjectMembersQueryHandler : IRequestHandler<GetSubjectMembersQuery, Result<PagedResult<SubjectMemberDto>>>
{
    private readonly TLearnDbContext _context;
    private readonly ILogger<GetSubjectMembersQueryHandler> _logger;

    public GetSubjectMembersQueryHandler(TLearnDbContext context, ILogger<GetSubjectMembersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<SubjectMemberDto>>> Handle(GetSubjectMembersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

            if (subject == null)
                return Result<PagedResult<SubjectMemberDto>>.Failure($"Subject with id '{request.SubjectId}' was not found.");

            // Check permission: only members can view member list
            if (subject.UserId != request.CurrentUserId && !subject.CanUserView(request.CurrentUserId))
                return Result<PagedResult<SubjectMemberDto>>.Failure("You don't have permission to view members of this subject.");

            var query = _context.SubjectMembers
                .Include(m => m.User)
                .Where(m => m.SubjectId == request.SubjectId);

            var totalCount = await query.CountAsync(cancellationToken);

            var members = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new SubjectMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = m.User.FullName ?? m.User.Email ?? string.Empty,
                    UserEmail = m.User.Email ?? string.Empty,
                    Permission = m.Permission.ToString(),
                    JoinedAt = m.JoinedAt
                })
                .ToListAsync(cancellationToken);

            // Add owner to the list
            var owner = new SubjectMemberDto
            {
                Id = Guid.Empty,
                UserId = subject.UserId,
                UserName = subject.User?.FullName ?? subject.User?.Email ?? "Owner",
                UserEmail = subject.User?.Email ?? string.Empty,
                Permission = "Owner",
                JoinedAt = subject.CreatedAt
            };

            var allMembers = new List<SubjectMemberDto> { owner };
            allMembers.AddRange(members);

            var result = new PagedResult<SubjectMemberDto>(allMembers, totalCount + 1, request.PageNumber, request.PageSize);
            return Result<PagedResult<SubjectMemberDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting subject members for subject {SubjectId}", request.SubjectId);
            return Result<PagedResult<SubjectMemberDto>>.Failure("An error occurred while retrieving members.");
        }
    }
}