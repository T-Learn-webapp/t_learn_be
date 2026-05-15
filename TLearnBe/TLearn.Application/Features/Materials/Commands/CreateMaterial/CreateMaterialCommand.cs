using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.CreateMaterial;

public class CreateMaterialCommand : IRequest<Result<LearningMaterialDto>>
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public Guid SubjectId { get; set; }
    public Guid UserId { get; set; }
}