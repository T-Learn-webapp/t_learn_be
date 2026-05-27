using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.GenerateFlashcardsByAi;

public class GenerateFlashcardsByAiCommand
    : IRequest<Result<List<AiGeneratedFlashcardDto>>>
{
    public Guid MaterialId { get; set; }

    public int Count { get; set; } = 10;
}