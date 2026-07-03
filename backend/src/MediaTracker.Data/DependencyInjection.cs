using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTracker.Data
{
    public static class DependencyInjection
    {
        public static void RegisterDataDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.RegisterRepositories();
            services.RegisterEfContext(configuration, environment);
        }

        private static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IMediaItemRepository, MediaItemRepository>(); 
        }

        private static void RegisterEfContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
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
