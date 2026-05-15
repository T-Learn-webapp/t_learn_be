using MediatR;
using TLearn.Common;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IRedisService _redisService;

    public LogoutCommandHandler(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        
        await _redisService.RevokeRefreshTokenAsync(request.UserId, request.RefreshToken);
        return Result<bool>.Success(true);
    }
}