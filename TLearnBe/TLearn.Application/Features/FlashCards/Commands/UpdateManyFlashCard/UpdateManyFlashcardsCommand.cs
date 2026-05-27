using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Application.Features.FlashCards.DTOs.Requests;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateManyFlashCard;

public class UpdateManyFlashcardsCommand
    : IRequest<Result<List<FlashcardDto>>>

{
    public Guid MaterialId { get; set; }
    public List<UpdateFlashcardItemRequest> Flashcards { get; set; } = new();
}