using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjectMembers;

public class GetSubjectMembersQuery : PaginationParams, IRequest<Result<PagedResult<SubjectMemberDto>>>
{
    public Guid SubjectId { get; set; }
    public Guid CurrentUserId { get; set; }
}