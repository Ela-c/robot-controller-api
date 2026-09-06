using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Dtos.Robot;
using robot_controller_api.Models;
using robot_controller_api.Services.Robot;
using System.Security.Claims;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/robot/state")]
[Authorize(Policy = "CatalogRead")]
public class RobotStateController : ControllerBase
{
    private readonly IRobotStateService _robotStateService;

    public RobotStateController(IRobotStateService robotStateService)
    {
        _robotStateService = robotStateService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RobotStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRobotState(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var state = await _robotStateService.GetOrCreateAsync(userId.Value, cancellationToken);
        return Ok(ToDto(state));
    }

    [HttpPut("map")]
    [ProducesResponseType(typeof(RobotStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectMap([FromBody] SelectRobotMapRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var state = await _robotStateService.SelectMapAsync(userId.Value, request.MapId, cancellationToken);
            return Ok(ToDto(state));
        }
        catch (RobotDomainException ex)
        {
            return ToProblem(ex);
        }
    }

    [HttpPut("position")]
    [ProducesResponseType(typeof(RobotStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PlaceRobot([FromBody] PlaceRobotRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var state = await _robotStateService.PlaceRobotAsync(userId.Value, request.X, request.Y, cancellationToken);
            return Ok(ToDto(state));
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

    private static RobotStateDto ToDto(RobotState state)
    {
        return new RobotStateDto
        {
            MapId = state.MapId,
            MapName = state.Map?.Name,
            X = state.X,
            Y = state.Y,
            HasPosition = state.X.HasValue && state.Y.HasValue,
            ModifiedDate = state.ModifiedDate
        };
    }
}
