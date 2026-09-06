using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.Robot;

namespace robot_controller_api.Tests.Services;

public class RobotStateServiceTests
{
    [Fact]
    public async Task SelectMap_ClearsExistingPosition()
    {
        await using var context = BuildContext();
        context.Users.Add(new User { Id = 1, Email = "u@a.com", FirstName = "A", LastName = "B", PasswordHash = "x", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        context.Maps.AddRange(
            new Map { Id = 1, Name = "M1", Rows = 5, Columns = 5, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            new Map { Id = 2, Name = "M2", Rows = 5, Columns = 5, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        context.RobotStates.Add(new RobotState { UserId = 1, MapId = 1, X = 2, Y = 2, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new RobotStateService(context, new RobotMovementService(), NullLogger<RobotStateService>.Instance);

        var state = await service.SelectMapAsync(1, 2, CancellationToken.None);

        Assert.Equal(2, state.MapId);
        Assert.Null(state.X);
        Assert.Null(state.Y);
    }

    [Fact]
    public async Task PlaceRobot_WithoutMap_ThrowsConflict()
    {
        await using var context = BuildContext();
        context.Users.Add(new User { Id = 1, Email = "u@a.com", FirstName = "A", LastName = "B", PasswordHash = "x", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new RobotStateService(context, new RobotMovementService(), NullLogger<RobotStateService>.Instance);

        var ex = await Assert.ThrowsAsync<RobotDomainException>(() => service.PlaceRobotAsync(1, 0, 0, CancellationToken.None));

        Assert.Equal(StatusCodes.Status409Conflict, ex.StatusCode);
    }

    [Fact]
    public async Task PlaceRobot_OutsideMap_ThrowsBadRequest()
    {
        await using var context = BuildContext();
        context.Users.Add(new User { Id = 1, Email = "u@a.com", FirstName = "A", LastName = "B", PasswordHash = "x", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        context.Maps.Add(new Map { Id = 1, Name = "M1", Rows = 2, Columns = 2, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        context.RobotStates.Add(new RobotState { UserId = 1, MapId = 1, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new RobotStateService(context, new RobotMovementService(), NullLogger<RobotStateService>.Instance);

        var ex = await Assert.ThrowsAsync<RobotDomainException>(() => service.PlaceRobotAsync(1, 5, 5, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    private static RobotContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<RobotContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RobotContext(options);
    }
}
