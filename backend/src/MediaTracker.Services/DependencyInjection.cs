using Microsoft.Extensions.DependencyInjection;

namespace MediaTracker.Services
{
    public static class DependencyInjection
    {
        public static void RegisterServiceDependencies(this IServiceCollection services)
        {
            services.RegisterServices();
        }

        private static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IMediaItemService, MediaItemService>();
        }
    }
}