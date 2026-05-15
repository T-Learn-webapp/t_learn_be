using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Subjects.Queries.GetMaterialsBySubject;

public class GetMaterialsBySubjectQuery : PaginationParams, IRequest<Result<PagedResult<LearningMaterialDto>>>
{
    public Guid SubjectId { get; set; }
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; }
}