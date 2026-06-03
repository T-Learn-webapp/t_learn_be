using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.FlashCards.Queries.GetFlashcardByMaterial;

public class GetFlashcardsByMaterialQuery
    : PaginationParams, IRequest<Result<PagedResult<FlashcardDto>>>
{
    public Guid MaterialId { get; set; }
    public string? SearchTerm { get; set; }
    public FlashcardLearningStatus? Status { get; set; }
}