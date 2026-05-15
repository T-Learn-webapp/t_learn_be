using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}