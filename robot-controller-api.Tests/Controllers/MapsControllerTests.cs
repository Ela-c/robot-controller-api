using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Controllers;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Tests.Controllers;

public class MapsControllerTests
{
    [Fact]
    public void GetAllMaps_ReturnsRepoMaps()
    {
        var expected = new List<Map> { CreateMap(1, "A", 5, 5) };
        var repo = new FakeMapRepo { GetAllMapsFunc = () => expected };
        var controller = new MapsController(repo);

        var result = controller.GetAllMaps();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSquareMapsOnly_ReturnsRepoMaps()
    {
        var expected = new List<Map> { CreateMap(2, "B", 3, 3) };
        var repo = new FakeMapRepo { GetSquareMapsFunc = () => expected };
        var controller = new MapsController(repo);

        var result = controller.GetSquareMapsOnly();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetMapById_WhenExists_ReturnsOk()
    {
        var map = CreateMap(7, "Main", 5, 6);
        var repo = new FakeMapRepo { GetByIdFunc = _ => map };
        var controller = new MapsController(repo);

        var result = controller.GetMapById(7);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(map, ok.Value);
    }

    [Fact]
    public void GetMapById_WhenMissing_ReturnsNotFound()
    {
        var repo = new FakeMapRepo { GetByIdFunc = _ => null };
        var controller = new MapsController(repo);

        var result = controller.GetMapById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void AddMap_WhenNull_ReturnsBadRequest()
    {
        var controller = new MapsController(new FakeMapRepo());

        var result = controller.AddMap(null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void AddMap_WhenNameEmpty_ReturnsBadRequest()
    {
        var controller = new MapsController(new FakeMapRepo());

        var result = controller.AddMap(CreateMap(0, string.Empty, 2, 2));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No map name provided", badRequest.Value);
    }

    [Fact]
    public void AddMap_WhenMapExists_ReturnsConflict()
    {
        var repo = new FakeMapRepo { GetByNameFunc = _ => CreateMap(3, "Office", 3, 4) };
        var controller = new MapsController(repo);

        var result = controller.AddMap(CreateMap(0, "Office", 5, 5));

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Map already exists", conflict.Value);
    }

    [Fact]
    public void AddMap_WhenInsertSucceeds_ReturnsCreatedAtRoute()
    {
        var inserted = CreateMap(11, "Warehouse", 6, 7);
        var repo = new FakeMapRepo
        {
            GetByNameFunc = _ => null,
            InsertFunc = _ => inserted
        };
        var controller = new MapsController(repo);

        var result = controller.AddMap(CreateMap(0, "Warehouse", 6, 7));

        var created = Assert.IsType<CreatedAtRouteResult>(result);
        Assert.Equal("GetMapById", created.RouteName);
        Assert.Equal(inserted, created.Value);
    }

    [Fact]
    public void AddMap_WhenInsertThrows_ReturnsBadRequest()
    {
        var repo = new FakeMapRepo
        {
            GetByNameFunc = _ => null,
            InsertFunc = _ => throw new InvalidOperationException("db")
        };
        var controller = new MapsController(repo);

        var result = controller.AddMap(CreateMap(0, "Warehouse", 6, 7));

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void AddMap_WhenInsertReturnsNull_ReturnsBadRequest()
    {
        var repo = new FakeMapRepo
        {
            GetByNameFunc = _ => null,
            InsertFunc = _ => null
        };
        var controller = new MapsController(repo);

        var result = controller.AddMap(CreateMap(0, "Warehouse", 6, 7));

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void UpdateMap_WhenNull_ReturnsBadRequest()
    {
        var controller = new MapsController(new FakeMapRepo());

        var result = controller.UpdateMap(1, null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void UpdateMap_WhenMissing_ReturnsNotFound()
    {
        var repo = new FakeMapRepo { GetByIdFunc = _ => null };
        var controller = new MapsController(repo);

        var result = controller.UpdateMap(2, CreateMap(2, "X", 2, 2));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void UpdateMap_WhenNameChangedAndTaken_ReturnsBadRequest()
    {
        var repo = new FakeMapRepo
        {
            GetByIdFunc = _ => CreateMap(2, "Old", 2, 2),
            GetByNameFunc = _ => CreateMap(9, "New", 5, 5)
        };
        var controller = new MapsController(repo);

        var result = controller.UpdateMap(2, CreateMap(2, "New", 2, 2));

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void UpdateMap_WhenUpdateSucceeds_ReturnsNoContent()
    {
        var repo = new FakeMapRepo
        {
            GetByIdFunc = _ => CreateMap(2, "Old", 2, 2),
            GetByNameFunc = _ => null,
            UpdateFunc = _ => 1
        };
        var controller = new MapsController(repo);

        var result = controller.UpdateMap(2, CreateMap(2, "New", 10, 12));

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void DeleteMap_WhenDeleteAffectsNotOneRow_ReturnsNotFound()
    {
        var repo = new FakeMapRepo { DeleteFunc = _ => 0 };
        var controller = new MapsController(repo);

        var result = controller.DeleteMap(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteMap_WhenDeleteSucceeds_ReturnsNoContent()
    {
        var repo = new FakeMapRepo { DeleteFunc = _ => 1 };
        var controller = new MapsController(repo);

        var result = controller.DeleteMap(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void CheckCoordintate_WhenMapMissing_ReturnsNotFound()
    {
        var repo = new FakeMapRepo { GetByIdFunc = _ => null };
        var controller = new MapsController(repo);

        var result = controller.CheckCoordintate(3, 1, 1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CheckCoordintate_WhenCoordinateInside_ReturnsTrue()
    {
        var repo = new FakeMapRepo { GetByIdFunc = _ => CreateMap(3, "M", 5, 5) };
        var controller = new MapsController(repo);

        var result = controller.CheckCoordintate(3, 1, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<bool>(ok.Value));
    }

    [Fact]
    public void CheckCoordintate_WhenCoordinateOutside_ReturnsFalse()
    {
        var repo = new FakeMapRepo { GetByIdFunc = _ => CreateMap(3, "M", 5, 5) };
        var controller = new MapsController(repo);

        var result = controller.CheckCoordintate(3, 0, -1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.False(Assert.IsType<bool>(ok.Value));
    }

    private static Map CreateMap(int id, string name, int rows, int columns) =>
        new(id, name, "desc", rows, columns, rows == columns, DateTime.UtcNow, DateTime.UtcNow);

    private sealed class FakeMapRepo : IMapDataAccess
    {
        public Func<int, int> DeleteFunc { get; set; } = _ => 1;
        public Func<List<Map>> GetAllMapsFunc { get; set; } = () => new List<Map>();
        public Func<int, Map?> GetByIdFunc { get; set; } = _ => null;
        public Func<string, Map?> GetByNameFunc { get; set; } = _ => null;
        public Func<List<Map>> GetSquareMapsFunc { get; set; } = () => new List<Map>();
        public Func<Map, Map?> InsertFunc { get; set; } = map => map;
        public Func<Map, int> UpdateFunc { get; set; } = _ => 1;

        public int DeleteMap(int inputId) => DeleteFunc(inputId);
        public List<Map> GetAllMaps() => GetAllMapsFunc();
        public Map? GetMapById(int inputId) => GetByIdFunc(inputId);
        public Map? GetMapByName(string inputName) => GetByNameFunc(inputName);
        public List<Map> GetSquareMaps() => GetSquareMapsFunc();
        public Map? InsertMap(Map map) => InsertFunc(map);
        public int UpdateMap(Map map) => UpdateFunc(map);
    }
}
