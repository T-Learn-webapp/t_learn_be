using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Payments.Commands;

public class HandlePayOSWebhookCommand : IRequest<Result<bool>>

{
    public string RawBody { get; set; } = string.Empty;
}