using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using Serilog;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/maps")]
public class MapsController : ControllerBase
{
	private readonly IMapDataAccess _mapRepo;
    public MapsController(IMapDataAccess mapRepo)
    {
        _mapRepo = mapRepo;
    }

    [AllowAnonymous]
    [HttpGet()]
	public IEnumerable<Map> GetAllMaps() => _mapRepo.GetAllMaps();

	[Authorize(Policy = "CatalogRead")]
    [HttpGet("square")]
	public IEnumerable<Map> GetSquareMapsOnly() => _mapRepo.GetSquareMaps();

	[Authorize(Policy = "CatalogRead")]
    [HttpGet("{id:int}", Name = "GetMapById")]
	public IActionResult GetMapById(int id)
	{
		Map? result = _mapRepo.GetMapById(id);
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
	public IActionResult AddMap(Map? map)
	{
		if (map == null)
		{
			return BadRequest();
		}
		else if (map.Name == "")
		{
			return BadRequest("No map name provided");
		}
		else if (_mapRepo.GetMapByName(map.Name) != null)
		{
			return Conflict("Map already exists");
		}

		Map newMap = new();
		var date = DateTime.Now;
		Map? result;
		try
		{
			newMap.Name = map.Name;
			newMap.Description = map.Description;
			newMap.Columns = map.Columns;
			newMap.Rows = map.Rows;
			newMap.CreatedDate = date;
			newMap.ModifiedDate = date;
			result = _mapRepo.InsertMap(newMap);
		}
		catch (Exception error)
		{
            Console.WriteLine(error);
            return BadRequest();
		}
		if (result == null)
		{
            Console.WriteLine("Error: Insertion of map failed");
            return BadRequest();
		}
		return CreatedAtRoute("GetMapById", new { id = result.Id }, result);
	}

	[Authorize(Policy = "CatalogWrite")]
    [HttpPut("{id:int}")]
	public IActionResult UpdateMap(int id, Map? map)
	{
		if (map == null)
		{
			return BadRequest();
		}

		// find map
		Map? storedMap = _mapRepo.GetMapById(id);
		if (storedMap == null)
		{
			return NotFound();
		}

		if(map.Name != storedMap.Name)
		{
			// check if name already exists
			Map? mapWithSameName = _mapRepo.GetMapByName(map.Name);
			if ( mapWithSameName != null)
			{
				Log.Information("(update map, action) Error: new map name already exists => Name: {name}", map.Name);
				return BadRequest();
			}
		}

		try
		{
			storedMap.Columns = map.Columns;
			storedMap.Rows = map.Rows; 
			storedMap.Name = map.Name;
			storedMap.Description = map.Description;
			storedMap.ModifiedDate = DateTime.Now;
			_mapRepo.UpdateMap(storedMap);
		}
		catch (Exception error)
		{
            Console.WriteLine(error);
            return BadRequest();
		}

		return NoContent();
	}

	[Authorize(Policy = "CatalogWrite")]
    [HttpDelete("{id}")]
	public IActionResult DeleteMap(int id)
	{
		int nRows = _mapRepo.DeleteMap(id);
		if (nRows != 1)
		{
            Console.WriteLine($"Error: Rows affected by delete operation: {nRows}");
            return NotFound();
		}
		return NoContent();
	}

	[Authorize(Policy = "CatalogRead")]
    [HttpGet("{id}/{x}-{y}")]
	public IActionResult CheckCoordintate(int id, int x, int y)
	{
		bool isOnMap = false;

		// find map
		Map? storedMap = _mapRepo.GetMapById(id);
		if (storedMap == null)
		{
			return NotFound();
		}

		// check if coordinates are inside map
		if(x < storedMap.Rows && y < storedMap.Columns && x > 0 && y > 0)
		{
			isOnMap = true;
		}

		return Ok(isOnMap);
	}
}
