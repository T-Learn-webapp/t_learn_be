using MediatR;
using TLearn.Application.Features.Payments.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Payments.Commands;

public class CreateUpgradePaymentCommand
    : IRequest<Result<CreateUpgradePaymentDto>>

{
    public string PlanType { get; set; } = string.Empty;
}