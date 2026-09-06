using Microsoft.Extensions.Configuration;

namespace robot_controller_api.Persistence
{
    internal static class DbConfig
    {
        private static readonly string _connectionString;

        static DbConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var config = builder.Build();
            _connectionString = config.GetConnectionString("RobotConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__RobotConnection")
                ?? Environment.GetEnvironmentVariable("RobotConnection")
                ?? string.Empty;
        }

        public static string ConnectionString => _connectionString;
    }
}
