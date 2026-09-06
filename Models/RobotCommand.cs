using System;
using System.Collections.Generic;

namespace robot_controller_api.Models
{
    public class RobotCommand
    {

	public RobotCommand()
	{
	}

	public RobotCommand(int id, string name, string? description, bool isMoveCommand, DateTime createdDate, DateTime modifiedDate)
		: this(id, name, description, isMoveCommand, null, createdDate, modifiedDate)
	{
	}

	public RobotCommand(int id, string name, string? description, bool isMoveCommand, MovementDirection? movementDirection, DateTime createdDate, DateTime modifiedDate)
	{
		this.Id = id;
		this.Name = name;
		this.Description = description;
		this.IsMoveCommand = isMoveCommand;
		this.MovementDirection = movementDirection;
		this.CreatedDate = createdDate;
		this.ModifiedDate = modifiedDate;
	}
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMoveCommand { get; set; }
		public MovementDirection? MovementDirection { get; set; }
	public ICollection<RobotCommandStep> Steps { get; set; } = new List<RobotCommandStep>();
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
