using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;
using TrialWorld.Infrastructure.MediaPlayback;

namespace TrialWorld.Infrastructure.Media
{
    public static class MediaPlayerRegistration
    {
        public static IServiceCollection AddMediaPlayer(this IServiceCollection services)
        {
            // Removed duplicate FFmpegMediaPlayer registration - already registered in InfrastructureServiceRegistration

            return services;
        }
    }
}