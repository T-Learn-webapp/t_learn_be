using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.FlashCards.Commands.CreateFlashCard;
using TLearn.Application.Features.FlashCards.Commands.CreateManyFlashcard;
using TLearn.Application.Features.FlashCards.Commands.DeleteFlashcard;
using TLearn.Application.Features.FlashCards.Commands.GenerateFlashcardsByAi;
using TLearn.Application.Features.FlashCards.Commands.UpdateFlashCard;
using TLearn.Application.Features.FlashCards.Commands.UpdateFlashCardProgress;
using TLearn.Application.Features.FlashCards.Commands.UpdateManyFlashCard;
using TLearn.Application.Features.FlashCards.DTOs.Requests;
using TLearn.Application.Features.FlashCards.Queries.GetDueFlashcards;
using TLearn.Application.Features.FlashCards.Queries.GetFlashcardByMaterial;
using TLearn.Application.Features.FlashCards.Queries.GetFlashcardDetail;

namespace TLearn.API.Controllers;

[Route("api/flashcards")]
[Authorize]
[ApiController]
public class FlashcardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlashcardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFlashcardCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("many")]
    public async Task<IActionResult> CreateMany(
        [FromBody] CreateManyFlashcardsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("generate-ai")]
    public async Task<IActionResult> GenerateByAi(
        [FromBody] GenerateFlashcardsByAiCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("material/{materialId:guid}")]
    public async Task<IActionResult> GetByMaterial(
        Guid materialId,
        [FromQuery] GetFlashcardsByMaterialQuery query,
        CancellationToken cancellationToken)

    {
        query.MaterialId = materialId;

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(
        Guid id,
        CancellationToken cancellationToken)

    {
        var result = await _mediator.Send(
            new GetFlashcardDetailQuery(id),
            cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDue(
        [FromQuery] GetDueFlashcardsQuery query,
        CancellationToken cancellationToken)

    {
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/content")]
    public async Task<IActionResult> UpdateContent(
        Guid id,
        [FromBody] UpdateFlashcardContentRequest request,
        CancellationToken cancellationToken)

    {
        var command = new UpdateFlashcardContentCommand

        {
            FlashcardId = id,

            Front = request.Front,

            Back = request.Back,

            Hint = request.Hint
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("many")]
    public async Task<IActionResult> UpdateMany(
        [FromBody] UpdateManyFlashcardsCommand command,
        CancellationToken cancellationToken)

    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/progress")]
    public async Task<IActionResult> UpdateProgress(
        Guid id,
        [FromBody] UpdateFlashcardProgressRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFlashcardProgressCommand
        {
            FlashcardId = id,
            Quality = request.Quality
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
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteFlashcardCommand(id),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}