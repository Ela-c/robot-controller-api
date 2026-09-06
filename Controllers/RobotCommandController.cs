using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;
using Serilog;

namespace robot_controller_api.Controllers
{
    [ApiController]
	[Route("api/robot-commands")]
	public class RobotCommandController : ControllerBase
	{
		private readonly IRobotCommandDataAccess _robotCommandsRepo;
		private readonly IRobotCommandService _robotCommandService;

		public RobotCommandController(IRobotCommandDataAccess robotCommandsRepo, IRobotCommandService robotCommandService)
        {
            _robotCommandsRepo = robotCommandsRepo;
			_robotCommandService = robotCommandService;
        }

        // Robot command endpoints
		[Authorize(Policy = "CatalogRead")]
        [HttpGet()]
		public IEnumerable<RobotCommand> GetAllRobotCommands()
		{
			return _robotCommandsRepo.GetRobotCommands();
		}

		[Authorize(Policy = "CatalogRead")]
        [HttpGet("move")]
		public IEnumerable<RobotCommand> GetMoveCommandsOnly()
		{
			return _robotCommandsRepo.GetMoveCommands();
		}

		[Authorize(Policy = "CatalogRead")]
        [HttpGet("{id:int}", Name = "GetRobotCommand")]
		[ProducesResponseType(typeof(RobotCommandStatusDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetRobotCommand(int id, CancellationToken cancellationToken)
		{
			RobotCommandStatusDto? result = await _robotCommandService.GetStatusAsync(id, cancellationToken);
			if (result != null)
			{
				return Ok(result);
			}
			else
			{
				return NotFound();
			}
		}

		[Authorize(Policy = "CatalogWrite")]
		[HttpPost()]
		[ProducesResponseType(typeof(RobotCommandSubmissionResponseDto), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> AddRobotCommand(RobotCommandSubmitRequestDto? inputCommand, CancellationToken cancellationToken)
		{
			if (inputCommand == null)
			{
				Log.Information("(add command, action) Error: input command is null");
				return BadRequest("no command provided");
			}

			try
			{
				RobotCommand result = await _robotCommandService.SubmitAsync(inputCommand, cancellationToken);
				var response = new RobotCommandSubmissionResponseDto
				{
					Id = result.Id,
					Status = result.Status.ToString(),
					StatusUrl = $"/api/robot-commands/{result.Id}"
				};

				return AcceptedAtRoute("GetRobotCommand", new { id = result.Id }, response);
			}
			catch (ArgumentException error)
			{
				Log.Information("(add command, action) Error: {error}", error.Message);
				return BadRequest(error.Message);
			}
			catch (InvalidOperationException error)
			{
				Log.Information("(add command, action) Error: {error}", error.Message);
				return Conflict(error.Message);
			}
			catch (Exception error)
			{
				Log.Information("(add command, action) Error: {error}", error);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}

		[Authorize(Policy = "CatalogWrite")]
        [HttpPut("{id:int}")]
		public IActionResult UpdateRobotCommand(int id, RobotCommand? command)
		{
			if(command == null)
			{
				Log.Information("(update command, action) Error: argument command is null");
                return BadRequest();
			}
			// find command
			RobotCommand? storedCmd = _robotCommandsRepo.GetRobotCommandById(id);
			if (storedCmd == null)
			{
				return NotFound();
			}

			if (storedCmd.Status != RobotCommandStatus.Pending && storedCmd.Status != RobotCommandStatus.Queued)
			{
				return Conflict("only pending or queued commands can be updated");
			}

			if(command.Name != storedCmd.Name)
			{
				// check if name already exists
				RobotCommand? commandWithSameName = _robotCommandsRepo.GetRobotCommandByName(command.Name);
				if(commandWithSameName != null) 
				{ 
					Log.Information("(update command, action) Error: new command name already exists => Name: {name}", command.Name);
					return BadRequest();
				}
			}

			try
			{
				storedCmd.IsMoveCommand = command.IsMoveCommand;
				storedCmd.Description = command.Description;
				storedCmd.Name = command.Name;
				storedCmd.ModifiedDate = DateTime.UtcNow;
				_robotCommandsRepo.UpdateRobotCommand(storedCmd);
			}
			catch (Exception error)
			{
				Log.Information("(update command, action) Error: {error}", error);
                return BadRequest();
			}

			return NoContent();
		}

		[Authorize(Policy = "CatalogWrite")]
		[HttpPost("{id:int}/cancel")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> CancelRobotCommand(int id, CancellationToken cancellationToken)
		{
			var result = await _robotCommandService.CancelAsync(id, cancellationToken);

			return result switch
			{
				RobotCommandCancellationResult.NotFound => NotFound(),
				RobotCommandCancellationResult.NotAllowed => Conflict("command cannot be cancelled in its current status"),
				_ => Ok()
			};
		}

		[Authorize(Policy = "CatalogWrite")]
        [HttpDelete("{id}")]
		public IActionResult DeleteRobotCommand(int id)
		{
			try
			{
				int rowsAffected = _robotCommandsRepo.DeleteRobotCommand(id);

				if(rowsAffected == 0)
				{
					return NotFound();
				}
			}
			catch (Exception error)
			{
				Log.Information("(delete command, action) Error: {error}", error);
				return BadRequest();
			}
			return NoContent();
		}
	}

}
