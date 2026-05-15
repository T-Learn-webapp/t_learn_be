using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Auth.Commands.Login;
using TLearn.Application.Features.Auth.Commands.Logout;
using TLearn.Application.Features.Auth.Commands.RefreshToken;
using TLearn.Application.Features.Auth.Commands.Register;
using TLearn.Application.Features.Auth.Commands.ResendVerification;
using TLearn.Application.Features.Auth.Commands.VerifyEmail;
using TLearn.Application.Features.Auth.DTOs;
using TLearn.Infrastructure.Services;

namespace TLearn.API.Controllers;
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;

    public AuthController(IMediator mediator , IRedisService redisService, IEmailService emailService)
    {
        _mediator = mediator;
        _redisService = redisService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        Console.WriteLine($"Received: {command.Email}, {command.FullName}");
    
        if (command == null)
            return BadRequest("Command is null");
    
        if (string.IsNullOrEmpty(command.Email))
            return BadRequest("Email is required");
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.IsUnauthorized)
                return Unauthorized(new { message = result.Error });
            return BadRequest(new { message = result.Error });
        }
        
        return Ok(result.Data);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.IsUnauthorized)
                return Unauthorized(new { message = result.Error });
            return BadRequest(new { message = result.Error });
        }
        
        return Ok(result.Data);
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var refreshToken = Request.Headers["RefreshToken"].ToString();
    
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Invalid request" });
    
        var command = new LogoutCommand
        {
            UserId = Guid.Parse(userId),
            RefreshToken = refreshToken
        };
    
        var result = await _mediator.Send(command);
    
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
    
        return Ok(new { message = "Logged out successfully" });
    }
    
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result)
            return BadRequest("Verify fail");
        
        return Ok(new { message = "Email verified successfully" });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result)
            return BadRequest("Resent Fail");
        
        return Ok(new { message = "New verification email sent." });
    }
    
  
}