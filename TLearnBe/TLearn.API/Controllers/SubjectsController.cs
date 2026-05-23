using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Materials.Queries.GetMaterialsBySubject;
using TLearn.Application.Features.Subjects.Commands.CreateSubject;
using TLearn.Application.Features.Subjects.Commands.DeleteSubject;
using TLearn.Application.Features.Subjects.Commands.RemoveMember;
using TLearn.Application.Features.Subjects.Commands.UpdateMemberPermission;
using TLearn.Application.Features.Subjects.Commands.UpdateSubject;
using TLearn.Application.Features.Subjects.DTOs;
using TLearn.Application.Features.Subjects.Queries.GetMaterialsBySubject;
using TLearn.Application.Features.Subjects.Queries.GetMySubjects;
using TLearn.Application.Features.Subjects.Queries.GetSubjectById;
using TLearn.Application.Features.Subjects.Queries.GetSubjectMembers;
using TLearn.Application.Features.Subjects.Queries.GetSubjects;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = true)
    {
        var userId = GetUserId();
        var query = new GetSubjectsQuery
        {
            UserId = userId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMySubjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = true,
        [FromQuery] bool? onlyPublic = null)
    {
        var userId = GetUserId();
        var query = new GetMySubjectsQuery
        {
            UserId = userId,
            SearchTerm = searchTerm,
            OnlyPublic = onlyPublic,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Data);
    }

    

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubject(Guid id)
    {
        var userId = GetUserId();
        var query = new GetSubjectByIdQuery { Id = id, UserId = userId };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpGet("{id}/materials")]
    public async Task<IActionResult> GetSubjectMaterials(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = true)
    {
        var userId = GetUserId();
        var query = new GetMaterialsBySubjectQuery
        {
            SubjectId = id,
            UserId = userId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectCommand command)
    {
        command.UserId = GetUserId();
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectCommand command)
    {
        command.Id = id;
        command.UserId = GetUserId();
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var command = new DeleteSubjectCommand { Id = id, UserId = GetUserId() };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return NoContent();
    }
    
    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var query = new GetSubjectMembersQuery
        {
            SubjectId = id,
            CurrentUserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    // PUT: api/subjects/{subjectId}/members/{memberId}/permission
    [HttpPut("{subjectId}/members/{memberId}/permission")]
    public async Task<IActionResult> UpdateMemberPermission(Guid subjectId, Guid memberId, [FromBody] UpdateMemberPermissionDto request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var command = new UpdateMemberPermissionCommand
        {
            SubjectId = subjectId,
            MemberId = memberId,
            Permission = request.Permission,
            CurrentUserId = userId
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Permission updated successfully" });
    }

    // DELETE: api/subjects/{subjectId}/members/{memberId}
    [HttpDelete("{subjectId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(Guid subjectId, Guid memberId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        
        var command = new RemoveMemberCommand
        {
            SubjectId = subjectId,
            MemberId = memberId,
            CurrentUserId = userId
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Member removed successfully" });
    }
    

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return Guid.Parse(userIdClaim);
    }
}