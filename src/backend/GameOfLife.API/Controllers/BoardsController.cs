using GameOfLife.API.DTOs;
using GameOfLife.API.Services;
using GameOfLife.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _svc;

    public BoardsController(IBoardService svc) => _svc = svc;

    [HttpPost]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken ct)
    {
        try
        {
            var entity = await _svc.CreateAsync(request, ct);
            var dto = _svc.ToDto(entity);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("invalid_request", ex.Message));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken ct)
    {
        try
        {
            var entity = await _svc.GetAsync(id, ct);
            return Ok(_svc.ToDto(entity));
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new ErrorResponse("not_found", knf.Message));
        }
    }

    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateBoardRequest req, CancellationToken ct)
    {
        try
        {
            var entity = await _svc.UpdateAsync(id, req, ct);
            return Ok(_svc.ToDto(entity));
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new ErrorResponse("not_found", knf.Message));
        }
        catch (ArgumentException bad)
        {
            return BadRequest(new ErrorResponse("invalid_request", bad.Message));
        }
    }

    [HttpPost("{id}/next")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Next([FromRoute] string id, [FromBody] NextRequest req, CancellationToken ct)
    {
        try
        {
            var (entity, result) = await _svc.NextAsync(id, req, ct);
            var dto = _svc.ToDto(entity, stateOverride: req.Persist ? null : result, ruleOverrideNotation: req.Rule);
            return Ok(dto);
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new ErrorResponse("not_found", knf.Message));
        }
        catch (ArgumentException bad)
        {
            return BadRequest(new ErrorResponse("invalid_request", bad.Message));
        }
    }

    [HttpPost("{id}/advance")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Advance([FromRoute] string id, [FromBody] AdvanceRequest req, CancellationToken ct)
    {
        try
        {
            var (entity, result) = await _svc.AdvanceAsync(id, req, ct);
            var dto = _svc.ToDto(entity, stateOverride: req.Persist ? null : result, ruleOverrideNotation: req.Rule);
            return Ok(dto);
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new ErrorResponse("not_found", knf.Message));
        }
        catch (ArgumentException bad)
        {
            return BadRequest(new ErrorResponse("invalid_request", bad.Message));
        }
    }

    [HttpPost("{id}/final")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)] 
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)] 
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Final([FromRoute] string id, [FromBody] FinalRequest req, CancellationToken ct)
    {
        try
        {
            var (entity, result) = await _svc.FinalAsync(id, req, ct);

            switch (result.Status)
            {
                case ConclusionStatus.Stable:
                case ConclusionStatus.Extinct:
                    var dto = _svc.ToDto(entity, stateOverride: req.Persist ? null : result.Last, ruleOverrideNotation: req.Rule);
                    return Ok(dto);

                case ConclusionStatus.CycleDetected:
                    return Conflict(new ErrorResponse(
                        "cycle_detected",
                        "Board did not converge to a final state (oscillator or spaceship).",
                        new { period = result.Period, stepsTaken = result.StepsTaken }));

                case ConclusionStatus.AttemptsExceeded:
                    return UnprocessableEntity(new ErrorResponse(
                        "attempts_exceeded",
                        $"No conclusion found within {req.MaxAttempts} attempts.",
                        new { stepsTaken = result.StepsTaken }));

                default:
                    return Problem("Unknown conclusion status.");
            }
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new ErrorResponse("not_found", knf.Message));
        }
        catch (ArgumentException bad)
        {
            return BadRequest(new ErrorResponse("invalid_request", bad.Message));
        }
    }
}