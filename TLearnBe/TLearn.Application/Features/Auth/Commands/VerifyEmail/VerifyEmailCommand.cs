using MediatR;

namespace TLearn.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

