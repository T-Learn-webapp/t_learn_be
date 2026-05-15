using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Queries.GetMaterialById;

public class GetMaterialByIdQuery : IRequest<Result<LearningMaterialDto>>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
}