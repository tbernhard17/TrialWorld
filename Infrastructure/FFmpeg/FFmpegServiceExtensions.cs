using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;
using TrialWorld.Infrastructure.FFmpeg.Services;
using TrialWorld.Infrastructure.Models.FFmpeg;
using TrialWorld.Infrastructure.Utilities;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Extension methods for registering FFmpeg-related services.
    /// </summary>
    public static class FFmpegServiceExtensions
    {
        /// <summary>
        /// Adds FFmpeg-related services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFFmpegServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure FFmpeg options
            services.Configure<FFmpegOptions>(configuration.GetSection("FFmpeg"));
            
            // Register process runner
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            
            // Register silence detection service
            services.AddSingleton<ISilenceDetectionService, SilenceDetectionService>();
            
            return services;
        }
    }
}
