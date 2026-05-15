using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.UpdateMaterial;

public class UpdateMaterialCommand : IRequest<Result<LearningMaterialDto>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public Guid UserId { get; set; }
}