using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using robot_controller_api.Hubs;
using robot_controller_api.Authentication;
using robot_controller_api.Persistence;
using robot_controller_api.Services.Robot;
using robot_controller_api.Services.RobotCommands;
using Serilog;
using System.Security.Claims;

namespace robot_controller_api
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddSerilog((services, lc) => lc
				.ReadFrom.Configuration(builder.Configuration));

			builder.Services.AddControllers();
			builder.Services.AddSignalR();
			
            builder.Services.AddDbContext<RobotContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("RobotConnection")));

			// Add Authentication Service
			builder.Services
				.AddAuthentication("SessionAuthentication")
				.AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("SessionAuthentication", default);

            // Add Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
				options.AddPolicy("CatalogRead", policy =>
					policy.RequireClaim(ClaimTypes.Role, "admin", "user"));

				options.AddPolicy("CatalogWrite", policy =>
					policy.RequireClaim(ClaimTypes.Role, "admin"));

				options.AddPolicy("UserAdmin", policy =>
					policy.RequireClaim(ClaimTypes.Role, "admin"));

				options.AddPolicy("SelfOrAdmin", policy =>
					policy.RequireAssertion(context =>
					{
						if (context.User.HasClaim(ClaimTypes.Role, "admin"))
						{
							return true;
						}

						var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
						if (string.IsNullOrWhiteSpace(userIdClaim))
						{
							return false;
						}

						string? routeId = null;

						if (context.Resource is HttpContext httpContext)
						{
							routeId = httpContext.GetRouteValue("id")?.ToString();
						}
						else if (context.Resource is AuthorizationFilterContext mvcContext)
						{
							routeId = mvcContext.RouteData.Values["id"]?.ToString();
						}

						return !string.IsNullOrWhiteSpace(routeId)
							&& string.Equals(userIdClaim, routeId, StringComparison.Ordinal);
					}));
            });


            builder.Services.AddScoped<IMapDataAccess, MapEF>();
			builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandEF>();
			builder.Services.AddScoped<IRobotCommandService, RobotCommandService>();
			builder.Services.AddScoped<IRobotCommandExecutor, RobotCommandExecutor>();
			builder.Services.AddScoped<IRobotMovementService, RobotMovementService>();
			builder.Services.AddScoped<IRobotStateService, RobotStateService>();
			builder.Services.AddScoped<IRobotSequenceService, RobotSequenceService>();
			builder.Services.AddScoped<IRobotSequenceExecutor, RobotSequenceExecutor>();
			builder.Services.AddSingleton<IRobotUpdateNotifier, SignalRRobotUpdateNotifier>();
			builder.Services.AddSingleton<IRobotCommandQueue, RobotCommandQueue>();
			builder.Services.AddSingleton<IRobotSequenceQueue, RobotSequenceQueue>();
			builder.Services.Configure<RobotExecutionOptions>(builder.Configuration.GetSection("RobotExecution"));
			builder.Services.AddHostedService<RobotCommandBackgroundService>();
			builder.Services.AddHostedService<RobotSequenceBackgroundService>();
            builder.Services.AddScoped<IUserDataAccess, UserEF>();
			builder.Services.AddScoped<ISessionDataAccess, SessionEF>();
			builder.Services.AddSingleton<SessionTokenService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

            app.UseAuthentication();
            
			app.UseAuthorization();

			app.UseStaticFiles();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger(c =>
				{
					c.PreSerializeFilters.Add((swagger, httpReq) =>
					{
						var user = httpReq.HttpContext.User;

						if (user?.Identity?.IsAuthenticated == true)
						{
							var username =
								user.Identity.Name
								?? user.FindFirst(ClaimTypes.Name)?.Value
								?? user.FindFirst("name")?.Value
								?? "Unknown";

							var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value)
								.Concat(user.FindAll("role").Select(r => r.Value))
								.Concat(user.FindAll("roles").Select(r => r.Value))
								.Distinct()
								.ToArray();

							var roleText = roles.Length > 0 ? string.Join(", ", roles) : "None";
							swagger.Info.Description = $"<div><div><strong>Logged in as:</strong> {username}</div><div><strong>Roles:</strong> {roleText}</div></div>";
						}
						else
						{
							swagger.Info.Description = "<div><div><strong>Logged in as:</strong> Anonymous</div><div><strong>Roles:</strong> None</div></div>";
						}
					});
				});

				app.UseSwaggerUI(c =>
				{
					c.InjectStylesheet("/swagger-ui/user-info.css");
				});
			}

            app.UseHttpsRedirection();

			app.MapControllers();
			app.MapHub<RobotHub>("/hubs/robot");

			app.Run();
		}
	}
}