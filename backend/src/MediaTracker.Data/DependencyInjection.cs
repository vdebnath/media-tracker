using MediaTracker.Data.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTracker.Data
{
    /// <summary>
    /// DI class to help register Data Project interfaces and its implementations
    /// Also helps to establish the SQLite connection through appsettings config
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers the Data layer's services into the DI container
        /// Calls methods with injected params to perform all data and db configurations 
        /// </summary>
        /// <param name="services">Collection of services for application to compose, Program.cs</param>
        /// <param name="configuration">Collection of configurations for application to compose, Program.cs</param>
        /// <param name="environment">Provides info on web hosting env being used, Program.cs</param>
        public static void RegisterDataDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.RegisterRepositories();
            services.RegisterEfContext(configuration, environment);
        }

        private static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IMediaItemRepository, MediaItemRepository>(); 
            services.AddScoped<IMediaItemSecurityAuth, MediaItemSecurityAuth>();
        }

        private static void RegisterEfContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            //Ensure the SQLite DB directory exists before EF opens the connection
            var dbDirectoryConfig = configuration["Database:Directory"]
                ?? throw new InvalidOperationException("Missing configuration: Database:Directory"); 

            var dbFileNameConfig = configuration["Database:FileName"]
                ?? throw new InvalidOperationException("Missing configuration: Database:FileName");

            var dbDirectory = Path.Combine(environment.ContentRootPath, dbDirectoryConfig);
            Directory.CreateDirectory(dbDirectory);

            var dbPath = Path.Combine(dbDirectory, dbFileNameConfig);
            var connectionString = $"Data Source={dbPath}";

            services.AddDbContext<MediaTrackerDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
        }
    }
}
