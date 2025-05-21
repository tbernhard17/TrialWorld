using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Common.Interfaces;
using TrialWorld.Core.Common.Services;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Services.Common;
using TrialWorld.Core.Services;

namespace TrialWorld.Core.Services
{
    public static class ServiceLifetimeManager
    {
        private static readonly IReadOnlyDictionary<Type, ServiceLifetime> ServiceLifetimes = new Dictionary<Type, ServiceLifetime>
        {
            // Keep only potentially valid Core interfaces
            // { typeof(IFFmpegService), ServiceLifetime.Singleton },            // REMOVED
            // { typeof(IMediaPlayer), ServiceLifetime.Singleton },             // REMOVED (Likely Presentation)
            // { typeof(ISearchIndexer), ServiceLifetime.Singleton },        // REMOVED (Type not found)
            // { typeof(IEmotionAnalysisService), ServiceLifetime.Singleton }, // REMOVED
            // { typeof(IThumbnailExtractor), ServiceLifetime.Singleton },      // REMOVED (Type not found)
            // { typeof(IPoseDetectionService), ServiceLifetime.Singleton },   // REMOVED
            
            // { typeof(IMediaProcessingPipeline), ServiceLifetime.Scoped }, // REMOVED
            // { typeof(ISearchService), ServiceLifetime.Scoped },           // REMOVED (Type not found)
            
            // { typeof(IMediaEnhancementService), ServiceLifetime.Transient },  // REMOVED (Type not found)
            // { typeof(IFileDialogService), ServiceLifetime.Transient }       // REMOVED (Likely Presentation)
            //{ typeof(IEventAggregator), ServiceLifetime.Singleton }, // Removed: Ambiguous and dictionary unused
            //{ typeof(IContentAnalysisService), ServiceLifetime.Transient }, // Removed: Implementation belongs elsewhere
            //{ typeof(ISearchService), ServiceLifetime.Transient },          // Removed: Implementation belongs elsewhere
            //{ typeof(ITranscriptionService), ServiceLifetime.Transient }    // Removed: Implementation belongs elsewhere
        }; // Dictionary might be entirely unused now.

        public static void RegisterService<TInterface, TImplementation>(IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (!ServiceLifetimes.TryGetValue(typeof(TInterface), out var lifetime))
            {
                lifetime = ServiceLifetime.Scoped; // Default to scoped if not specified
            }

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TInterface, TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TInterface, TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TInterface, TImplementation>();
                    break;
            }
        }

        public static void AddCoreServices(this IServiceCollection services)
        {
            // Register Core Services
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<ICentralStateService, CentralStateService>();
            services.AddSingleton<TrialWorld.Core.Common.Interfaces.IEventAggregator, TrialWorld.Core.Services.Common.EventAggregator>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ILoggingService, SerilogLoggingService>();
            
            // Assuming implementations exist elsewhere (e.g., Infrastructure)
            // services.AddScoped<IContentAnalysisService, ContentAnalysisService>(); // Removed: Register in Composition Root
            // services.AddScoped<ISearchService, SearchService>();                   // Removed: Register in Composition Root
            // services.AddScoped<ITranscriptionService, TranscriptionService>();       // Removed: Register in Composition Root
            
            // ... commented out services ...
        }
    }
}