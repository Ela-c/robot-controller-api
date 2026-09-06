using robot_controller_api.Models;

namespace robot_controller_api.Tests.Models;

public class ModelTests
{
    [Fact]
    public void RobotCommand_Constructor_MapsAllProperties()
    {
        var created = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modified = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        var model = new RobotCommand(1, "LEFT", "Turn left", true, created, modified);

        Assert.Equal(1, model.Id);
        Assert.Equal("LEFT", model.Name);
        Assert.Equal("Turn left", model.Description);
        Assert.True(model.IsMoveCommand);
        Assert.Equal(created, model.CreatedDate);
        Assert.Equal(modified, model.ModifiedDate);
    }

    [Fact]
    public void RobotCommand_DefaultConstructor_AllowsPropertySetters()
    {
        var now = DateTime.UtcNow;
        var model = new RobotCommand
        {
            Id = 2,
            Name = "REPORT",
            Description = "Status",
            IsMoveCommand = false,
            CreatedDate = now,
            ModifiedDate = now
        };

        Assert.Equal(2, model.Id);
        Assert.Equal("REPORT", model.Name);
        Assert.Equal("Status", model.Description);
        Assert.False(model.IsMoveCommand);
        Assert.Equal(now, model.CreatedDate);
        Assert.Equal(now, model.ModifiedDate);
    }

    [Fact]
    public void Map_Constructor_MapsAllProperties()
    {
        var created = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var modified = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc);

        var model = new Map(3, "Arena", "Play area", 10, 12, false, created, modified);

        Assert.Equal(3, model.Id);
        Assert.Equal("Arena", model.Name);
        Assert.Equal("Play area", model.Description);
        Assert.Equal(10, model.Rows);
        Assert.Equal(12, model.Columns);
        Assert.False(model.IsSquare);
        Assert.Equal(created, model.CreatedDate);
        Assert.Equal(modified, model.ModifiedDate);
    }

    [Fact]
    public void Map_DefaultConstructor_AllowsPropertySetters()
    {
        var now = DateTime.UtcNow;
        var model = new Map
        {
            Id = 4,
            Name = "Grid",
            Description = "desc",
            Rows = 8,
            Columns = 8,
            IsSquare = true,
            CreatedDate = now,
            ModifiedDate = now
        };

        Assert.Equal(4, model.Id);
        Assert.Equal("Grid", model.Name);
        Assert.Equal("desc", model.Description);
        Assert.Equal(8, model.Rows);
        Assert.Equal(8, model.Columns);
        Assert.True(model.IsSquare);
        Assert.Equal(now, model.CreatedDate);
        Assert.Equal(now, model.ModifiedDate);
    }

    [Fact]
    public void User_Constructor_MapsAllProperties()
    {
        var created = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var modified = new DateTime(2024, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        var model = new User(5, "user@example.com", "Jane", "Doe", "hash", "admin user", "Admin", created, modified);

        Assert.Equal(5, model.Id);
        Assert.Equal("user@example.com", model.Email);
        Assert.Equal("Jane", model.FirstName);
        Assert.Equal("Doe", model.LastName);
        Assert.Equal("hash", model.PasswordHash);
        Assert.Equal("admin user", model.Description);
        Assert.Equal("Admin", model.Role);
        Assert.Equal(created, model.CreatedDate);
        Assert.Equal(modified, model.ModifiedDate);
    }

    [Fact]
    public void User_DefaultConstructor_AllowsPropertySetters()
    {
        var now = DateTime.UtcNow;
        var model = new User
        {
            Id = 6,
            Email = "new@example.com",
            FirstName = "John",
            LastName = "Smith",
            PasswordHash = "hash2",
            Description = null,
            Role = "User",
            CreatedDate = now,
            ModifiedDate = now
        };

        Assert.Equal(6, model.Id);
        Assert.Equal("new@example.com", model.Email);
        Assert.Equal("John", model.FirstName);
        Assert.Equal("Smith", model.LastName);
        Assert.Equal("hash2", model.PasswordHash);
        Assert.Null(model.Description);
        Assert.Equal("User", model.Role);
        Assert.Equal(now, model.CreatedDate);
        Assert.Equal(now, model.ModifiedDate);
    }
}
