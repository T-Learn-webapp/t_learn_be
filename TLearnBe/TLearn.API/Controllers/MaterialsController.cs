using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Materials.Commands.CreateMaterial;
using TLearn.Application.Features.Materials.Commands.DeleteMaterial;
using TLearn.Application.Features.Materials.Commands.UpdateMaterial;
using TLearn.Application.Features.Materials.Queries.GetMaterialById;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaterialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMaterial(Guid id)
    {
        var userId = GetUserId();
        var query = new GetMaterialByIdQuery { Id = id, UserId = userId };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Data);
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
        
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(Guid id)
    {
        var command = new DeleteMaterialCommand { Id = id, UserId = GetUserId() };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return Guid.Parse(userIdClaim);
    }
}