using MediatR;
using TLearn.Application.Features.MaterialVersions.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.MaterialVersions.Queries.GetDetaisVersion;

public class GetMaterialVersionDetailQuery
    : IRequest<Result<MaterialVersionDetailDto>>
{
    public Guid MaterialId { get; set; }

    public Guid VersionId { get; set; }

    public Guid UserId { get; set; }
}