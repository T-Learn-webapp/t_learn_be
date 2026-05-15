using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRedisService _redisService;

    public RefreshTokenCommandHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IRedisService redisService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _redisService = redisService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Result<AuthResponse>.Unauthorized("Invalid access token");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Result<AuthResponse>.Unauthorized("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<AuthResponse>.Unauthorized("User not found");

        // Validate refresh token
        var isValid = await _redisService.IsRefreshTokenValidAsync(user.Id, request.RefreshToken);
        if (!isValid)
            return Result<AuthResponse>.Unauthorized("Invalid refresh token");

        // Revoke old refresh token
        await _redisService.RevokeRefreshTokenAsync(user.Id, request.RefreshToken);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Save new refresh token
        await _redisService.SetRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7));

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                SubscriptionType = user.SubscriptionType ?? "Free",
                EmailConfirmed = user.EmailConfirmed
            }
        });
    }
}