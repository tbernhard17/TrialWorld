using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Media.Models;
using TrialWorld.Infrastructure.MediaEnhancement.Services;

namespace TrialWorld.Infrastructure.MediaEnhancement
{
    /// <summary>
    /// Extension methods for registering media enhancement services with the DI container.
    /// </summary>
    public static class MediaEnhancementServiceRegistration
    {
        /// <summary>
        /// Adds media enhancement services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddMediaEnhancementServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register MediaEnhancementOptions
            // services.Configure<MediaEnhancementOptions>(
            //     configuration.GetSection("MediaEnhancement")); // Commented out due to missing type

            // Register Media Enhancement Service
            // services.AddTransient<IMediaEnhancementService, MediaEnhancementService>(); // Commented out due to missing type
            services.AddTransient<ISmartThumbnailService, SmartThumbnailService>();

            return services;
        }
    }
}