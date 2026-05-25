using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Application.Features.FlashCards.DTOs.Requests;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.CreateManyFlashcard;

public class CreateManyFlashcardsCommand 
    : IRequest<Result<List<FlashcardDto>>>
{
    public Guid MaterialId { get; set; }

    public List<CreateFlashcardItemRequest> Flashcards { get; set; } = new();
}