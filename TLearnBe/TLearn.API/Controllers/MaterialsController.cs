using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TLearn.Application.Features.Materials.Commands.CreateMaterial;
using TLearn.Application.Features.Materials.Commands.DeleteMaterial;
using TLearn.Application.Features.Materials.Commands.UpdateMaterial;
using TLearn.Application.Features.Materials.Commands.UpdateTitle;
using TLearn.Application.Features.Materials.DTOs;
using TLearn.Application.Features.Materials.Queries.GetMaterialById;
using TLearn.Common;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TLearnDbContext _context;
    public MaterialsController(IMediator mediator,TLearnDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMaterial(Guid id)
    {
        var userId = GetUserId();
        var query = new GetMaterialByIdQuery { Id = id, UserId = userId };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialCommand command)
    {
        command.UserId = GetUserId();
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, [FromBody] UpdateMaterialCommand command)
    {
        command.Id = id;
        command.UserId = GetUserId();
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/info")]
    public async Task<IActionResult> UpdateInfo(
        Guid id,
        [FromBody] UpdateMaterialInfoRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                message = "Chưa đăng nhập"
            });
        }

        var command = new UpdateMaterialInfoCommand
        {
            MaterialId = id,
            UserId = Guid.Parse(userId),
            Title = request.Title,
            Summary = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeleteLearningMaterialCommand(id),
            ct);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    
    [HttpGet("{id}/collaboration-info")]
    public async Task<IActionResult> GetCollaborationInfo(Guid id)
    {
        var userId = GetUserId();
        var material = await _context.LearningMaterials
            .Include(m => m.Subject)
            .FirstOrDefaultAsync(m => m.Id == id);
    
        if (material == null)
            return NotFound();
    
        // Check permission
        if (!material.Subject.CanUserView(userId))
            return Forbid();
    
        // Generate SignalR token
        var hubToken = GenerateHubToken(userId.ToString(), material.Id.ToString());
        var result = Result<object>.Success(new
        {
            materialId = material.Id,
            materialTitle = material.Title,
            subjectId = material.SubjectId,
            hubUrl = "/collaborationHub",
            hubToken = hubToken,
            version = material.Version,
            snapshot = material.YjsSnapshot,  // Initial snapshot
            isCollaborative = material.IsCollaborative
        });
        // return Ok(new
        // {
        //     materialId = material.Id,
        //     materialTitle = material.Title,
        //     subjectId = material.SubjectId,
        //     hubUrl = "/collaborationHub",
        //     hubToken = hubToken,
        //     version = material.Version,
        //     snapshot = material.YjsSnapshot,  // Initial snapshot
        //     isCollaborative = material.IsCollaborative
        // });
        return Ok(result);
    }

    [HttpPut("{id}/collaborative")]
    public async Task<IActionResult> ToggleCollaborative(Guid id, [FromBody] bool isCollaborative)
    {
        var userId = GetUserId();
        var material = await _context.LearningMaterials
            .Include(m => m.Subject)
            .FirstOrDefaultAsync(m => m.Id == id);
    
        if (material == null)
            return NotFound();
    
        if (material.UserId != userId && !material.Subject.CanUserManage(userId))
            return Forbid();
    
        material.IsCollaborative = isCollaborative;
        await _context.SaveChangesAsync();
    
        return Ok(new { isCollaborative = material.IsCollaborative });
    }

    private string GenerateHubToken(string userId, string materialId)
    {
        // Simple token for SignalR connection
        // In production, use JWT or built-in SignalR access token
        var payload = $"{userId}|{materialId}|{DateTime.UtcNow.Ticks}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return Guid.Parse(userIdClaim);
    }
}