using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Configuration; // Added for settings models
using TrialWorld.Infrastructure;
using TrialWorld.Presentation.ViewModels; // Added for MainWindowViewModel
using TrialWorld.Presentation.ViewModels.Search; // Added for SearchService

namespace TrialWorld.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
// (Removed invalid static constructor and debug statements)
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;
        private ILogger<App>? _logger;

        public ServiceProvider? ServiceProvider => _serviceProvider;

        /// <summary>
        /// Handles the application startup event
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();

            _logger.LogInformation("Application starting up");

            try
            {
                // Create MainWindow and set its DataContext manually
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = CreateMainWindowViewModel(_serviceProvider);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to start application");
                MessageBox.Show($"An error occurred during startup: {ex.Message}", "Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(-1);
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Configures the services for dependency injection
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
       {
           // Configuration
           IConfiguration configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

           // Register configuration
           services.AddSingleton(configuration);

           // Register strongly-typed configuration settings
           // AssemblyAI settings are configured in appsettings.json
           services.Configure<TranscriptionPathSettings>(configuration.GetSection("TranscriptionPaths"));

            // Register logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

            // Register infrastructure services
            services.AddInfrastructureServices(configuration);

            // Register Presentation/Application services (from the moved logic)
            services.AddApplicationServices(configuration);

            // Register Presentation services
            services.AddSingleton<Interfaces.INotificationService, Services.NotificationService>();
            
            // Register UI components and ViewModels
            services.AddTransient<MainWindow>();
            
            // Register presentation services from the extension methods
            // Use both service registration methods to ensure all services are registered
            ServiceRegistration.AddPresentationServices(services, configuration);
            PresentationServiceRegistration.AddPresentationServices(services, configuration);
            
        }

        /// <summary>
        /// Creates a MainWindowViewModel instance with all its dependencies
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>A MainWindowViewModel instance</returns>
        private MainWindowViewModel CreateMainWindowViewModel(ServiceProvider serviceProvider)
        {
            // Get all required services from the service provider
            var fileDialogService = serviceProvider.GetRequiredService<Interfaces.IFileDialogService>();
            var navigationService = serviceProvider.GetRequiredService<Interfaces.INavigationService>();
            var notificationService = serviceProvider.GetRequiredService<Interfaces.INotificationService>();
            var themeService = serviceProvider.GetRequiredService<Interfaces.IThemeService>();
            var windowManager = serviceProvider.GetRequiredService<Interfaces.IWindowManager>();
            var versionService = serviceProvider.GetRequiredService<Interfaces.IVersionService>();
            var resourceMonitorService = serviceProvider.GetRequiredService<Interfaces.IResourceMonitorService>();
            var transcriptionService = serviceProvider.GetRequiredService<Core.Interfaces.ITranscriptionService>();
            var databaseLoader = serviceProvider.GetRequiredService<Core.Interfaces.IDatabaseLoaderService>();
            var transcriptionVerification = serviceProvider.GetRequiredService<Core.Interfaces.ITranscriptionVerificationService>();
            var appSettingsService = serviceProvider.GetRequiredService<Core.Common.Interfaces.IAppSettingsService>();
            // Create the correct SearchService implementation that MainWindowViewModel expects
            var searchService = new ViewModels.SearchService();
            var silenceDetectionService = serviceProvider.GetRequiredService<Core.Interfaces.ISilenceDetectionService>();
            
            // Create logger instances for each view model
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var courtroomMediaPlayerLogger = loggerFactory.CreateLogger<ViewModels.CourtroomMediaPlayerViewModel>();
            var mediaTranscriptViewModelLogger = loggerFactory.CreateLogger<ViewModels.MediaTranscriptViewModel>();
            var mainWindowViewModelLogger = loggerFactory.CreateLogger<ViewModels.MainWindowViewModel>();
            
            // Create the MainWindowViewModel with all dependencies
            return new ViewModels.MainWindowViewModel(
                fileDialogService,
                navigationService,
                notificationService,
                themeService,
                windowManager,
                versionService,
                resourceMonitorService,
                transcriptionService,
                databaseLoader,
                transcriptionVerification,
                appSettingsService,
                searchService,
                silenceDetectionService,
                courtroomMediaPlayerLogger,
                mediaTranscriptViewModelLogger,
                mainWindowViewModelLogger);
        }

        /// <summary>
        /// Handles the application exit event
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            if (_logger != null)
            {
                _logger.LogInformation("Application shutting down");
            }

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Handles unhandled exceptions
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_logger != null)
            {
                _logger.LogError(e.Exception, "Unhandled exception occurred");
            }

            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);

            // Mark as handled to prevent application crash
            e.Handled = true;
        }
    }
}

