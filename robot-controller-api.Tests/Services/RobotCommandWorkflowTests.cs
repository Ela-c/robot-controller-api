using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Services;

public class RobotCommandWorkflowTests
{
    [Fact]
    public async Task SubmitCommand_CreatesDefinitionWithCompositeSteps()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();

        var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
        {
            Name = "MOVE-SHAPE",
            IsMoveCommand = true,
            MovementDirections = [MovementDirection.Right, MovementDirection.Up, MovementDirection.Up]
        }, CancellationToken.None);

        Assert.True(created.Id > 0);

        var status = await service.GetStatusAsync(created.Id, CancellationToken.None);
        Assert.NotNull(status);
        Assert.Equal([MovementDirection.Right, MovementDirection.Up, MovementDirection.Up], status!.MovementDirections);
    }

    [Fact]
    public async Task CancelCommandDefinition_WhenExists_ReturnsNotAllowed()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();

        var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
        {
            Name = "REPORT",
            IsMoveCommand = false
        }, CancellationToken.None);

        var result = await service.CancelAsync(created.Id, CancellationToken.None);

        Assert.Equal(RobotCommandCancellationResult.NotAllowed, result);
    }

    private static ServiceProvider BuildProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<RobotContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IRobotCommandService, RobotCommandService>(_ =>
            new RobotCommandService(_.GetRequiredService<RobotContext>(), NullLogger<RobotCommandService>.Instance));

        return services.BuildServiceProvider();
    }
}
