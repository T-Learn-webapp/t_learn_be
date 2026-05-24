using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Subjects.Queries.GetSubjects;

public class GetSubjectsQuery
    : PaginationParams, IRequest<Result<PagedResult<SubjectDto>>>

{
    public Guid? UserId { get; set; }

    public string? SearchTerm { get; set; }

    public SubjectFilterType Filter { get; set; } = SubjectFilterType.All;
}

public enum SubjectFilterType
{
    All = 1,
    Owned = 2,
    Joined = 3
}