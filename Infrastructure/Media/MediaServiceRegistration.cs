using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;
using TrialWorld.Infrastructure.MediaPlayback;

namespace TrialWorld.Infrastructure.Media
{
    public static class MediaServiceRegistration
    {
        public static IServiceCollection AddMediaServices(this IServiceCollection services)
        {
            // Removed duplicate FFmpegMediaPlayer registration - already registered in InfrastructureServiceRegistration
            // services.AddSingleton<IMediaEnhancementService, FFmpegMediaEnhancementService>(); // Commented out due to missing interface

            return services;
        }
    }
}