using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Dtos.Robot;
using robot_controller_api.Models;
using robot_controller_api.Services.Robot;
using System.Security.Claims;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/robot/sequences")]
[Authorize(Policy = "CatalogRead")]
public class RobotSequencesController : ControllerBase
{
    private readonly IRobotSequenceService _robotSequenceService;

    public RobotSequencesController(IRobotSequenceService robotSequenceService)
    {
        _robotSequenceService = robotSequenceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RobotSequenceSubmissionResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitSequence([FromBody] RobotSequenceSubmitRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _robotSequenceService.SubmitAsync(userId.Value, new RobotSequenceSubmitRequest
            {
                CommandIds = request.CommandIds,
                TargetX = request.TargetX,
                TargetY = request.TargetY
            }, cancellationToken);

            var response = new RobotSequenceSubmissionResponseDto
            {
                SequenceId = result.Sequence.Id,
                Status = result.Sequence.Status.ToString(),
                StartPosition = new RobotCoordinateDto { X = result.Sequence.StartX, Y = result.Sequence.StartY },
                PredictedFinalPosition = new RobotCoordinateDto { X = result.PredictedFinalPosition.X, Y = result.PredictedFinalPosition.Y },
                TotalSteps = result.Sequence.TotalSteps,
                StatusUrl = $"/api/robot/sequences/{result.Sequence.Id}"
            };

            return AcceptedAtAction(nameof(GetSequenceById), new { id = result.Sequence.Id }, response);
        }
        catch (RobotDomainException ex)
        {
            return ToProblem(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RobotSequenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSequences([FromQuery] RobotSequenceStatus? status, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var sequences = await _robotSequenceService.ListForUserAsync(userId.Value, status, cancellationToken);
        return Ok(sequences.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}", Name = "GetRobotSequenceById")]
    [ProducesResponseType(typeof(RobotSequenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSequenceById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var sequence = await _robotSequenceService.GetByIdForUserAsync(userId.Value, id, cancellationToken);
        if (sequence == null)
        {
            return NotFound();
        }

        return Ok(ToDto(sequence));
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSequence(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var sequence = await _robotSequenceService.RequestCancelAsync(userId.Value, id, cancellationToken);
            return Ok(new { id = sequence.Id, status = sequence.Status.ToString(), cancellationRequested = sequence.CancellationRequested });
        }
        catch (RobotDomainException ex)
        {
            return ToProblem(ex);
        }
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private ObjectResult ToProblem(RobotDomainException ex)
    {
        var details = new ProblemDetails
        {
            Status = ex.StatusCode,
            Title = ex.Title,
            Detail = ex.Message
        };

        foreach (var extension in ex.Extensions)
        {
            details.Extensions[extension.Key] = extension.Value;
        }

        return StatusCode(ex.StatusCode, details);
    }

    private static RobotSequenceDto ToDto(RobotCommandSequence sequence)
    {
        return new RobotSequenceDto
        {
            Id = sequence.Id,
            Status = sequence.Status.ToString(),
            MapId = sequence.MapId,
            StartX = sequence.StartX,
            StartY = sequence.StartY,
            FinalX = sequence.FinalX,
            FinalY = sequence.FinalY,
            CurrentStep = sequence.CurrentStep,
            TotalSteps = sequence.TotalSteps,
            CancellationRequested = sequence.CancellationRequested,
            CreatedDate = sequence.CreatedDate,
            StartedDate = sequence.StartedDate,
            CompletedDate = sequence.CompletedDate,
            FailureReason = sequence.FailureReason,
            Items = sequence.Items
                .OrderBy(item => item.Order)
                .Select(item => new RobotSequenceItemDto
                {
                    Order = item.Order,
                    CommandId = item.RobotCommandId,
                    CommandName = item.CommandName,
                    Executed = item.IsExecuted,
                    ExecutedDate = item.ExecutedDate
                })
                .ToList()
        };
    }
}
