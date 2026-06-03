using MediatR;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Payments.Commands.SyncPaymentStatus;

public class SyncPaymentStatusCommand
    : IRequest<Result<PaymentStatusDto>>

{
    public string OrderCode { get; set; } = string.Empty;
}