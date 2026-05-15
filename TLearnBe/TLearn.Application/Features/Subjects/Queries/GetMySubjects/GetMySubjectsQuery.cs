using MediatR;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Subjects.Queries.GetMySubjects;

public class GetMySubjectsQuery : PaginationParams, IRequest<Result<PagedResult<SubjectDto>>>
{
    public Guid UserId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? OnlyPublic { get; set; }
}