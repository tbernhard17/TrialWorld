using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Services;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Configuration;
using TrialWorld.Infrastructure.FFmpeg;
using TrialWorld.Infrastructure.FFmpeg.Services;
using TrialWorld.Infrastructure.MediaIngestion;
using TrialWorld.Infrastructure.Utilities;
using TrialWorld.Infrastructure.Search;
using TrialWorld.Infrastructure.Media;
using TrialWorld.Infrastructure.MediaPlayback;
using TrialWorld.Infrastructure.Export;
using TrialWorld.Infrastructure.BackgroundJobs;
using TrialWorld.Core.Interfaces.Services;
using TrialWorld.Core.Media.Models;
using TrialWorld.Infrastructure.MediaIngestion.Services;
using TrialWorld.Application.Services;
using TrialWorld.Infrastructure.Services;
using TrialWorld.Infrastructure.Transcription;

namespace TrialWorld.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure options
            services.Configure<FFmpegOptions>(configuration.GetSection("FFmpeg"));
            services.Configure<FolderMonitorOptions>(configuration.GetSection("FolderMonitor"));
            services.Configure<ExportOptions>(configuration.GetSection("Export"));
            
            // Note: MediaEnhancement features have been removed from the application
            
            // Register FFmpeg services using the extension method
            services.AddFFmpegServices(configuration);

            // Register Process Runner
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            
            // Register FFmpeg Path Resolver
            services.AddSingleton<IFFmpegPathResolver, FFmpegPathResolver>();

            // Register Media Info Service
            services.AddSingleton<IMediaInfoService, FFprobeMediaInfoService>();

            // Register Audio Extractor Service
            services.AddSingleton<TrialWorld.Core.Interfaces.IAudioExtractorService, FFmpegAudioExtractor>();

            // Register Media Trimmer Service
            services.AddSingleton<IMediaTrimmerService, FFmpegMediaTrimmer>();

            // Register Media Filter Service
            services.AddSingleton<IMediaFilterService, FFmpegMediaFilterService>();

            // Register Media Conversion Service
            services.AddSingleton<IMediaConversionService, FFmpegMediaConverter>();

            // Register Waveform Service
            services.AddSingleton<IWaveformService, FFmpegWaveformService>();

            // Register Media Concatenator Service
            services.AddSingleton<IMediaConcatenatorService, FFmpegMediaConcatenator>();

            // Register GIF Creation Service
            services.AddSingleton<IGifCreationService, FFmpegGifCreator>();

            // Register Frame Extractor Service
            services.AddSingleton<IFrameExtractorService, FFmpegFrameExtractor>();

            // Register core infrastructure services
            services.AddSingleton<IFFmpegService, FFmpegService>();
            services.AddSingleton<IFFmpegRealTimeService, FFmpegRealTimeService>();
            services.AddSingleton<ISilenceDetectionService, FFmpeg.Services.SilenceDetectionService>();
            services.AddSingleton<IMediaProcessingPipeline, MediaProcessingPipeline>();
            services.AddSingleton<IThumbnailExtractor, FFmpegThumbnailExtractor>();
            
            // Register transcription services using the direct API integration
            // The old AssemblyAI SDK implementation has been completely removed
            services.AddTranscriptionServices(configuration);
            
            // Register export service
            services.AddSingleton<IExportService, ExportService>();

            // Register Search Infrastructure Services
            TrialWorld.Infrastructure.Search.SearchServiceRegistration.AddInfrastructureSearchServices(services, configuration);

            // Register ISearchService implementation consistently
            services.AddSingleton<ISearchService, LuceneSearchEngine>();
            
            // Register MediaIndexingServiceAdapter as Singleton to avoid lifetime issues
            services.AddSingleton<IMediaIndexingService, Media.MediaIndexingServiceAdapter>();
            
            // Register MediaService
            services.AddSingleton<IMediaService, Services.MediaService>();

            // Register background services
            services.AddSingleton<IBackgroundTaskManager, BackgroundTaskManager>();
            services.AddHostedService(sp => (BackgroundTaskManager)sp.GetRequiredService<IBackgroundTaskManager>());
            services.AddHostedService<MediaQueueMonitorService>();
            services.AddHostedService<FolderMonitorService>();

            return services;
        }
    }
}