using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Config;
using TrialWorld.Core.Interfaces;
// using TrialWorld.Infrastructure.Adapters; // Commented out: Directory is empty, causing build errors
using TrialWorld.Infrastructure.MediaIngestion.Interfaces;
using TrialWorld.Infrastructure.MediaIngestion.Services;
using TrialWorld.Infrastructure.MediaIngestion.Services.Adapters;
using TrialWorld.Infrastructure.Services;

namespace TrialWorld.Infrastructure.MediaIngestion
{
    /// <summary>
    /// Extensions for registering media ingestion services with dependency injection
    /// </summary>
    public static class MediaIngestionServiceRegistration
    {
        /// <summary>
        /// Add media ingestion services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddMediaIngestionServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get feature flags to determine which implementations to use
            var featureFlags = configuration.GetFeatureFlags();

            // Register core implementations
            services.AddTransient<Core.Interfaces.IMediaProcessingPipeline, MediaProcessingPipeline>();

            // Register infrastructure implementations with adapters based on feature flags
            if (featureFlags.UseNewMediaProcessingPipeline)
            {
                // When feature flag is enabled, register the Core implementation
                services.AddTransient<Core.Interfaces.IMediaProcessingPipeline, TrialWorld.Infrastructure.MediaIngestion.Services.MediaProcessingPipeline>();

                // Register the new MediaProcessingServiceAdapter for the QUEUED interface
                services.AddTransient<IQueuedMediaIngestionService>(sp =>
                {
                    // Return an adapter that wraps the IMediaProcessingService implementation
                    var mediaProcessingService = sp.GetRequiredService<IMediaProcessingService>();
                    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MediaProcessingServiceAdapter>>();
                    return new MediaProcessingServiceAdapter(mediaProcessingService, logger);
                });
            }
            else
            {
                // When feature flag is disabled, register both implementations
                // Register core implementation
                services.AddTransient<Core.Interfaces.IMediaProcessingPipeline, TrialWorld.Infrastructure.MediaIngestion.Services.MediaProcessingPipeline>();

                // Register the Infrastructure's DIRECT processor service
                services.AddTransient<IDirectMediaProcessor, TrialWorld.Infrastructure.MediaIngestion.Services.MediaProcessingPipeline>();
            }

            // Register other ingestion services that don't depend on feature flags
            services.AddTransient<TrialWorld.Core.Interfaces.IMediaContentIndexerService, MediaContentIndexerService>();

            // Register IMetadataFileReader for DI (always available)
            services.AddTransient<IMetadataFileReader, TrialWorld.Infrastructure.FFmpeg.Services.EnhancedSearchEngine>();

            // Register Media Ingestion related services
            services.AddTransient<IDirectMediaProcessor, MediaProcessingPipeline>();
            
            // Register TranscriptionService
            services.AddTransient<ITranscriptionService, TrialWorld.Infrastructure.Transcription.Services.TranscriptionService>();

            return services;
        }
    }
}