using MediatR;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Payments.Queries.GetPaymentStatus;

public class GetPaymentStatusQuery
    : IRequest<Result<PaymentStatusDto>>

{
    public string OrderCode { get; set; } = string.Empty;
}