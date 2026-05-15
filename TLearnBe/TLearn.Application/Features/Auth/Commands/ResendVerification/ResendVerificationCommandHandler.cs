using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Auth.Commands.ResendVerification;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
{
    private readonly UserManager<User> _userManager;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;
    private readonly string _frontendUrl;

    public ResendVerificationCommandHandler(
        UserManager<User> userManager,
        IRedisService redisService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _redisService = redisService;
        _emailService = emailService;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
    }

    public async Task<bool> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return false;

        if (user.EmailConfirmed)
            return false;
    
        var newToken = Guid.NewGuid().ToString();
        var key = $"email_verify:{request.Email}";
        await _redisService.SetAsync(key, newToken, TimeSpan.FromHours(24));
    
        var verificationLink = $"{_frontendUrl}/verify-email?email={request.Email}&token={newToken}";
        await _emailService.SendVerificationEmail(request.Email, verificationLink);
    
        return true;
    }
}