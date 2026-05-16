using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Subjects.Commands.AcceptInvitation;
using TLearn.Application.Features.Subjects.Commands.InviteMember;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Application.Features.Subjects.Queries.GetInvitationInfo;
using TLearn.Domain.Entities;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/invitations/info?token=xxx
    [HttpGet("info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInvitationInfo([FromQuery] string token)
    {
        var query = new GetInvitationInfoQuery { Token = token };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Data);
    }

    // POST: api/invitations/subjects/{subjectId}/invite
    [HttpPost("subjects/{subjectId}/invite")]
    [Authorize]
    public async Task<IActionResult> InviteMember(Guid subjectId, [FromBody] InviteMemberDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var command = new InviteMemberCommand
        {
            SubjectId = subjectId,
            Email = request.Email,
            Permission = request.Permission,
            InvitedBy = userId
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = $"Invitation sent to {request.Email}" });
    }

    // POST: api/invitations/accept
    [HttpPost("accept")]
    [Authorize]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var command = new AcceptInvitationCommand
        {
            Token = request.Token,
            UserId = userId,
            RegisterData = null
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new
        {
            message = "You have joined the subject successfully!",
            subjectId = result.Data.SubjectId,
            subjectName = result.Data.SubjectName
        });
    }

    // POST: api/invitations/accept-with-registration
    [HttpPost("accept-with-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitationWithRegistration([FromBody] AcceptInvitationWithRegistrationRequest request)
    {
        var command = new AcceptInvitationCommand
        {
            Token = request.Token,
            UserId = null,
            RegisterData = new RegisterData
            {
                Password = request.Password,
                FullName = request.FullName
            }
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new
        {
            message = "Account created and invitation accepted! Please login.",
            subjectId = result.Data.SubjectId,
            subjectName = result.Data.SubjectName,
            email = result.Data.Email
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    public class AcceptInvitationWithRegistrationRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}