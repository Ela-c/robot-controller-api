using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace robot_controller_api.Migrations
{
    /// <inheritdoc />
    public partial class AddRobotStateAndSequenceExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "movement_direction",
                table: "robot_command",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "robot_command_sequence",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    map_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_x = table.Column<int>(type: "integer", nullable: false),
                    start_y = table.Column<int>(type: "integer", nullable: false),
                    final_x = table.Column<int>(type: "integer", nullable: true),
                    final_y = table.Column<int>(type: "integer", nullable: true),
                    current_step = table.Column<int>(type: "integer", nullable: false),
                    total_steps = table.Column<int>(type: "integer", nullable: false),
                    cancellation_requested = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    started_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    completed_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    modified_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_robot_command_sequence", x => x.id);
                    table.ForeignKey(
                        name: "FK_robot_command_sequence_map_map_id",
                        column: x => x.map_id,
                        principalTable: "map",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_robot_command_sequence_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "robot_state",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    map_id = table.Column<int>(type: "integer", nullable: true),
                    x = table.Column<int>(type: "integer", nullable: true),
                    y = table.Column<int>(type: "integer", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_robot_state", x => x.id);
                    table.ForeignKey(
                        name: "FK_robot_state_map_map_id",
                        column: x => x.map_id,
                        principalTable: "map",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_robot_state_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "robot_command_sequence_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    sequence_id = table.Column<int>(type: "integer", nullable: false),
                    robot_command_id = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_executed = table.Column<bool>(type: "boolean", nullable: false),
                    executed_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    command_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    movement_direction = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_robot_command_sequence_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_robot_command_sequence_item_robot_command_robot_command_id",
                        column: x => x.robot_command_id,
                        principalTable: "robot_command",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_robot_command_sequence_item_robot_command_sequence_sequence~",
                        column: x => x.sequence_id,
                        principalTable: "robot_command_sequence",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_robot_command_movement_direction",
                table: "robot_command",
                sql: "(is_move_command = false AND movement_direction IS NULL) OR (is_move_command = true AND movement_direction IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_robot_command_sequence_active_user",
                table: "robot_command_sequence",
                column: "user_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ix_robot_command_sequence_map_id",
                table: "robot_command_sequence",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_robot_command_sequence_item_robot_command_id",
                table: "robot_command_sequence_item",
                column: "robot_command_id");

            migrationBuilder.CreateIndex(
                name: "ix_robot_command_sequence_item_sequence_order",
                table: "robot_command_sequence_item",
                columns: new[] { "sequence_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_robot_state_map_id",
                table: "robot_state",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "ix_robot_state_user_id",
                table: "robot_state",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "robot_command_sequence_item");

            migrationBuilder.DropTable(
                name: "robot_state");

            migrationBuilder.DropTable(
                name: "robot_command_sequence");

            migrationBuilder.DropCheckConstraint(
                name: "ck_robot_command_movement_direction",
                table: "robot_command");

            migrationBuilder.DropColumn(
                name: "movement_direction",
                table: "robot_command");
        }
    }
}
