using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public partial class RobotContext : DbContext
    {
        public virtual DbSet<Map> Maps { get; set; } = null!;
        public virtual DbSet<RobotCommand> RobotCommands { get; set; } = null!;
        public virtual DbSet<RobotCommandSequence> RobotCommandSequences { get; set; } = null!;
        public virtual DbSet<RobotCommandSequenceItem> RobotCommandSequenceItems { get; set; } = null!;
        public virtual DbSet<RobotState> RobotStates { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserSession> UserSessions { get; set; } = null!;

        public RobotContext()
        {
        }

        public RobotContext(DbContextOptions<RobotContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Map>(entity =>
            {
                entity.ToTable("map");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Columns).HasColumnName("columns");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.Description)
                    .HasMaxLength(800)
                    .HasColumnName("description");

                entity.Property(e => e.IsSquare)
                    .HasColumnName("is_square")
                    .HasComputedColumnSql("((rows > 0) AND (rows = columns))", true);

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Rows).HasColumnName("rows");
            });

            modelBuilder.Entity<RobotCommand>(entity =>
            {
                entity.ToTable("robot_command");

                entity.HasCheckConstraint(
                    "ck_robot_command_movement_direction",
                    "(is_move_command = false AND movement_direction IS NULL) OR (is_move_command = true AND movement_direction IS NOT NULL)");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.Description)
                    .HasMaxLength(800)
                    .HasColumnName("description");

                entity.Property(e => e.IsMoveCommand).HasColumnName("is_move_command");

                entity.Property(e => e.MovementDirection).HasColumnName("movement_direction");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.StartedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("started_date");

                entity.Property(e => e.CompletedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("completed_date");

                entity.Property(e => e.FailureReason)
                    .HasMaxLength(800)
                    .HasColumnName("failure_reason");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<RobotCommandSequence>(entity =>
            {
                entity.ToTable("robot_command_sequence");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.MapId).HasColumnName("map_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.StartX).HasColumnName("start_x");

                entity.Property(e => e.StartY).HasColumnName("start_y");

                entity.Property(e => e.FinalX).HasColumnName("final_x");

                entity.Property(e => e.FinalY).HasColumnName("final_y");

                entity.Property(e => e.CurrentStep).HasColumnName("current_step");

                entity.Property(e => e.TotalSteps).HasColumnName("total_steps");

                entity.Property(e => e.CancellationRequested).HasColumnName("cancellation_requested");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.StartedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("started_date");

                entity.Property(e => e.CompletedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("completed_date");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.Property(e => e.FailureReason)
                    .HasMaxLength(800)
                    .HasColumnName("failure_reason");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_robot_command_sequence_user_id");

                entity.HasIndex(e => e.MapId)
                    .HasDatabaseName("ix_robot_command_sequence_map_id");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_robot_command_sequence_active_user")
                    .HasFilter("status IN (1, 2)")
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Map)
                    .WithMany()
                    .HasForeignKey(e => e.MapId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RobotCommandSequenceItem>(entity =>
            {
                entity.ToTable("robot_command_sequence_item");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.SequenceId).HasColumnName("sequence_id");

                entity.Property(e => e.RobotCommandId).HasColumnName("robot_command_id");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.IsExecuted).HasColumnName("is_executed");

                entity.Property(e => e.ExecutedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("executed_date");

                entity.Property(e => e.CommandName)
                    .HasMaxLength(50)
                    .HasColumnName("command_name");

                entity.Property(e => e.MovementDirection).HasColumnName("movement_direction");

                entity.HasIndex(e => new { e.SequenceId, e.Order })
                    .HasDatabaseName("ix_robot_command_sequence_item_sequence_order")
                    .IsUnique();

                entity.HasOne(e => e.Sequence)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.SequenceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RobotCommand)
                    .WithMany()
                    .HasForeignKey(e => e.RobotCommandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RobotState>(entity =>
            {
                entity.ToTable("robot_state");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.MapId).HasColumnName("map_id");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_robot_state_user_id")
                    .IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Map)
                    .WithMany()
                    .HasForeignKey(e => e.MapId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.Description)
                    .HasMaxLength(800)
                    .HasColumnName("description");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(200)
                    .HasColumnName("password_hash");

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasColumnName("role");
            });

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("user_session");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.Property(e => e.TokenHash)
                    .HasMaxLength(200)
                    .HasColumnName("token_hash");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_date");

                entity.Property(e => e.ExpiresDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("expires_date");

                entity.Property(e => e.RevokedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("revoked_date");

                entity.Property(e => e.LastSeenDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("last_seen_date");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
