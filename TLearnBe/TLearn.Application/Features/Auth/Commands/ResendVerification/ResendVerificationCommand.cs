using MediatR;

namespace TLearn.Application.Features.Auth.Commands.ResendVerification;

public class ResendVerificationCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
}