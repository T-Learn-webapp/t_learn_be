using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Services;

namespace TLearn.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IRedisService redisService,
        IEmailService emailService,
        IConfiguration config)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _redisService = redisService;
        _emailService = emailService;
        _config = config;
    }
    
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra email tồn tại
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new Exception("Email already in use.");

        // Tạo user
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            SubscriptionType = "Free",
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Tạo token và lưu vào Redis (hết hạn sau 24h)
        var verificationToken = Guid.NewGuid().ToString();
        var key = $"email_verify:{request.Email}";
        await _redisService.SetAsync(key, verificationToken, TimeSpan.FromHours(24));

        // Gửi email
        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";
        var verificationLink = $"{frontendUrl}/verify-email?email={request.Email}&token={verificationToken}";
        await _emailService.SendVerificationEmail(request.Email, verificationLink);

        // Tạo token đăng nhập
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _redisService.SetRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                SubscriptionType = user.SubscriptionType,
                EmailConfirmed = user.EmailConfirmed
            }
        };
    }
}