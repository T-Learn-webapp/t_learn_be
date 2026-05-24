using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.DeleteMaterial;

public class DeleteLearningMaterialCommand

    : IRequest<Result<bool>>

{

    public Guid MaterialId { get; set; }

    public DeleteLearningMaterialCommand(Guid materialId)

    {

        MaterialId = materialId;

    }

}