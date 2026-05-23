using MediatR;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthResponse>>
{
   
    public string RefreshToken { get; set; } = string.Empty;
}