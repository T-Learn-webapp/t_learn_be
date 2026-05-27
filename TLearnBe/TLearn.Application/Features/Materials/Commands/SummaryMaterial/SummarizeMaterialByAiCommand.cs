using MediatR;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.SummaryMaterial;

public class SummarizeMaterialByAiCommand
    : IRequest<Result<LearningMaterialDto>>

{
    public Guid MaterialId { get; set; }
}