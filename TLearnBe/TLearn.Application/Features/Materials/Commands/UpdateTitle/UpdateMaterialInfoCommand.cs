using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.UpdateTitle;

public class UpdateMaterialInfoCommand
    : IRequest<Result<LearningMaterialDto>>
{
    public Guid MaterialId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

}