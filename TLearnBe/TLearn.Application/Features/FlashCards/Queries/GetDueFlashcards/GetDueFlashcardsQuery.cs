using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.FlashCards.Queries.GetDueFlashcards;

public class GetDueFlashcardsQuery
    : PaginationParams, IRequest<Result<PagedResult<FlashcardDetailsDto>>>
{
    public Guid? MaterialId { get; set; }

    public Guid? SubjectId { get; set; }
}