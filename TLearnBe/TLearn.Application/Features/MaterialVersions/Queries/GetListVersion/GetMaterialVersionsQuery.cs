using MediatR;
using TLearn.Application.Features.MaterialVersions.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.MaterialVersions.Queries.GetListVersion;

public class GetMaterialVersionsQuery
    : PaginationParams, IRequest<Result<PagedResult<MaterialVersionListDto>>>
{
    public Guid MaterialId { get; set; }

    public Guid UserId { get; set; }
}