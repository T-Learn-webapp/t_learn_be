using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.ResetProgress;

public class ResetFlashcardProgressCommand : IRequest<Result<bool>>

{
    public Guid MaterialId { get; set; }

    public ResetFlashcardProgressCommand(Guid materialId)

    {
        MaterialId = materialId;
    }
}