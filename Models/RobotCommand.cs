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
	{
		this.Id = id;
		this.Name = name;
		this.Description = description;
		this.IsMoveCommand = isMoveCommand;
		this.CreatedDate = createdDate;
		this.ModifiedDate = modifiedDate;
		this.Status = RobotCommandStatus.Pending;
	}
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMoveCommand { get; set; }
		public RobotCommandStatus Status { get; set; } = RobotCommandStatus.Pending;
        public DateTime CreatedDate { get; set; }
		public DateTime? StartedDate { get; set; }
		public DateTime? CompletedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
		public string? FailureReason { get; set; }
    }
}
