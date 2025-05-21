using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using TrialWorld.Contracts.Logging; 
using TrialWorld.Core.Interfaces; 
using TrialWorld.Infrastructure.Services; 
using TrialWorld.Infrastructure.Transcription.Services; // Direct API implementation namespace
using TrialWorld.Infrastructure.FFmpeg; // Add FFmpeg namespace
using TrialWorld.Presentation.Interfaces; 
using TrialWorld.Presentation.ViewModels; 
using TrialWorld.Presentation.ViewModels.Search;
using TrialWorld.Presentation.Services;


namespace TrialWorld.Presentation 
{
    public static class DependencyInjection 
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();

            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .Enrich.FromLogContext()
                
                
                
                .WriteTo.Console()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    Path.Combine(logsDirectory, "trialaiworldlog-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                    retainedFileCountLimit: 31) 
                .CreateLogger();

            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddSerilog(Log.Logger, dispose: true);
            });

            // Register infrastructure implementations for core interfaces
            services.AddSingleton<IFFmpegService, TrialWorld.Infrastructure.FFmpeg.FFmpegService>();
            
            // Use fixed registration that properly handles service registration with concrete types
            // This ensures only the direct API implementation is used (no SDK remnants)
            // Register transcription service using the new implementation
            services.AddSingleton<ITranscriptionService, TranscriptionService>();
            
            // Register FFmpeg services including silence detection
            services.AddFFmpegServices(configuration);
            
            // Register ISilenceDetectionService
            services.AddSingleton<ISilenceDetectionService, TrialWorld.Infrastructure.FFmpeg.Services.SilenceDetectionService>();
            
            // Explicitly decorate the direct transcription service with silence detection
            // Configure transcription service with silence detection
            services.AddSingleton<ITranscriptionService>(sp => {
                var directService = sp.GetRequiredService<TranscriptionService>();
                var silenceDetectionService = sp.GetRequiredService<ISilenceDetectionService>();
                var logger = sp.GetRequiredService<ILogger<TrialWorld.Infrastructure.Transcription.Services.TranscriptionServiceWithSilenceDetection>>();
                return new TrialWorld.Infrastructure.Transcription.Services.TranscriptionServiceWithSilenceDetection(directService, silenceDetectionService, logger);
            });
            
            // Other services
            services.AddSingleton<IDatabaseLoaderService, Infrastructure.Services.DatabaseLoaderService>();
            services.AddSingleton<ITranscriptionVerificationService, Infrastructure.Services.TranscriptionVerificationService>();
            

            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<CourtroomMediaPlayerViewModel>();
            services.AddTransient<TranscriptionViewModel>();
            services.AddTransient<SearchViewModel>();
            services.AddTransient<AnalysisViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register UI Services (implementations likely in Presentation.Services)
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<TrialWorld.Presentation.Interfaces.IDialogService, TrialWorld.Presentation.Services.DialogService>();
services.AddSingleton<IVersionService, VersionService>();
services.AddSingleton<IResourceMonitorService, ResourceMonitorService>();
            // Use the WPF-specific implementation for file dialogs
            services.AddSingleton<IFileDialogService, WpfFileDialogService>();
// Register CentralStateService for transcription services and others
// Register EventAggregator for CentralStateService and others
// Register LoggingService for CentralStateService and others
            services.AddSingleton<TrialWorld.Core.Common.Interfaces.IAppSettingsService, TrialWorld.Core.Common.Services.AppSettingsService>();
            services.AddSingleton<TrialWorld.Core.Common.Interfaces.ILoggingService, TrialWorld.Core.Common.Services.LoggingService>();
            services.AddSingleton<TrialWorld.Core.Common.Interfaces.IEventAggregator, TrialWorld.Core.Common.Services.EventAggregator>();
            services.AddSingleton<TrialWorld.Core.Common.Interfaces.ICentralStateService, TrialWorld.Core.Common.Services.CentralStateService>();

            // Register Views (optional, but can be useful)
            // services.AddTransient<SettingsView>(); // Example if needed


            // Ensure logs are flushed on application exit (This should also move to App.xaml.cs)
            // AppDomain.CurrentDomain.ProcessExit += (sender, e) => Log.CloseAndFlush();

            return services;
        }
    }
} 