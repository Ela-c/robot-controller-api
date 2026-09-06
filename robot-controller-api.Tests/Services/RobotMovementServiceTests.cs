using robot_controller_api.Models;
using robot_controller_api.Services.Robot;

namespace robot_controller_api.Tests.Services;

public class RobotMovementServiceTests
{
    private readonly RobotMovementService _service = new();

    [Theory]
    [InlineData(2, 2, MovementDirection.Up, 2, 1)]
    [InlineData(2, 2, MovementDirection.Down, 2, 3)]
    [InlineData(2, 2, MovementDirection.Left, 1, 2)]
    [InlineData(2, 2, MovementDirection.Right, 3, 2)]
    public void CalculateNextPosition_ReturnsExpected(int x, int y, MovementDirection direction, int expectedX, int expectedY)
    {
        var next = _service.CalculateNextPosition(new RobotPosition(x, y), direction);

        Assert.Equal(expectedX, next.X);
        Assert.Equal(expectedY, next.Y);
    }

    [Fact]
    public void ValidateSequence_WhenStepLeavesMap_ReturnsFailingStep()
    {
        var map = new Map { Id = 1, Name = "Map", Rows = 3, Columns = 3 };
        var commands = new List<RobotCommand>
        {
            new() { Id = 1, Name = "Move Left", IsMoveCommand = true, MovementDirection = MovementDirection.Left }
        };

        var result = _service.ValidateSequence(map, new RobotPosition(0, 0), commands, null, null);

        Assert.False(result.IsValid);
        Assert.Equal(1, result.FailingStep);
    }

    [Fact]
    public void ValidateSequence_WithTargetMismatch_ReturnsInvalid()
    {
        var map = new Map { Id = 1, Name = "Map", Rows = 5, Columns = 5 };
        var commands = new List<RobotCommand>
        {
            new() { Id = 1, Name = "Move Right", IsMoveCommand = true, MovementDirection = MovementDirection.Right }
        };

        var result = _service.ValidateSequence(map, new RobotPosition(1, 1), commands, 3, 1);

        Assert.False(result.IsValid);
        Assert.Equal("Movement sequence does not reach target", result.ErrorTitle);
    }
}
