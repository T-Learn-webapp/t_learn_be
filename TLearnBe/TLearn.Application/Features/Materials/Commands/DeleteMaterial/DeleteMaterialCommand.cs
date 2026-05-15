using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Materials.Commands.DeleteMaterial;

public class DeleteMaterialCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}