using System;
using System.Collections.Generic;

namespace robot_controller_api.Models
{
    public class Map
    {

	public Map()
	{
	}

	public Map(int id, string name, string? description, int rows, int columns, bool? isSquare, DateTime createdDate, DateTime modifiedDate)
	{
		this.Id = id;
		this.Name = name;
		this.Description = description;
		this.Rows = rows;
		this.Columns = columns;
		this.IsSquare = isSquare;
		this.CreatedDate = createdDate;
		this.ModifiedDate = modifiedDate;
	}
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool? IsSquare { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
