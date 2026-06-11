using MediatR;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.Application.Features.Payments.Queries.GetPaymentHistory;

public class GetPaymentHistoryQuery
    : PaginationParams, IRequest<Result<PagedResult<PaymentHistoryDto>>>

{
    public Guid UserId { get; set; }

    public string? Status { get; set; }

    public string? PlanType { get; set; }

    public string? Search { get; set; }
}