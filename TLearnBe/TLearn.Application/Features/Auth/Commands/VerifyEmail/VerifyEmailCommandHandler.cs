using MediatR;
using Microsoft.AspNetCore.Identity;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IRedisService _redisService;

    public VerifyEmailCommandHandler(UserManager<User> userManager, IRedisService redisService)
    {
        _userManager = userManager;
        _redisService = redisService;
    }
    
    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var key = $"email_verify:{request.Email}";
        var storedToken = await _redisService.GetAsync(key);
    
        if (string.IsNullOrEmpty(storedToken))
            throw new Exception("Verification link has expired");
    
        if (storedToken != request.Token)
            throw new Exception("Invalid verification token");
    
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("User not found");
    
        if (user.EmailConfirmed)
            throw new Exception("Email already verified");
    
        user.EmailConfirmed= true;
        var updateResult = await _userManager.UpdateAsync(user);
        
        if (!updateResult.Succeeded)
            throw new Exception("Failed to verify email");
    
        await _redisService.RemoveAsync(key);
    
        return true;
    }
}