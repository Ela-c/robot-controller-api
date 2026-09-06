using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace robot_controller_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRobotCommandRuntimeLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_date",
                table: "robot_command");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "robot_command");

            migrationBuilder.DropColumn(
                name: "started_date",
                table: "robot_command");

            migrationBuilder.DropColumn(
                name: "status",
                table: "robot_command");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "completed_date",
                table: "robot_command",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "robot_command",
                type: "character varying(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_date",
                table: "robot_command",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "robot_command",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
