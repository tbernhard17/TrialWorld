using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TrialWorld.AssemblyAIDiagnostic.Extensions;
using TrialWorld.AssemblyAIDiagnostic.ViewModels;
using TrialWorld.AssemblyAIDiagnostic.Views;

namespace TrialWorld.AssemblyAIDiagnostic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Handles the Startup event of the Application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            
            // Check for API key and prompt if needed
            if (!await EnsureApiKeyConfiguredAsync())
            {
                Shutdown();
                return;
            }

            // Create and show the main window
            var mainWindow = new TranscriptionView();
            mainWindow.Show();
        }
        
        /// <summary>
        /// Ensures the API key is configured.
        /// </summary>
        /// <returns>True if the API key is configured; otherwise, false.</returns>
        private async Task<bool> EnsureApiKeyConfiguredAsync()
        {
            try
            {
                var apiKeyManager = ServiceProvider.GetRequiredService<ApiKeyManager>();
                var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
                
                // Check if API key exists
                var apiKey = await apiKeyManager.GetAssemblyAIKeyAsync();
                if (string.IsNullOrEmpty(apiKey))
                {
                    logger.LogInformation("No API key found. Prompting user for API key.");
                    
                    // Show API key setup dialog
                    var dialog = new ApiKeySetupDialog(apiKeyManager, ServiceProvider.GetRequiredService<ILogger<ApiKeySetupDialog>>());
                    var result = dialog.ShowDialog();
                    
                    if (result != true)
                    {
                        logger.LogWarning("User cancelled API key setup.");
                        MessageBox.Show("An AssemblyAI API key is required to use this application.", "API Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    
                    // Get the updated API key
                    apiKey = await apiKeyManager.GetAssemblyAIKeyAsync();
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        logger.LogError("Failed to retrieve API key after setup.");
                        MessageBox.Show("Failed to retrieve API key. Please restart the application and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                
                // Update configuration with API key
                await apiKeyManager.UpdateConfigurationWithStoredKeyAsync(Configuration);
                logger.LogInformation("API key configured successfully.");
                
                return true;
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
                logger.LogError(ex, "Error ensuring API key is configured.");
                MessageBox.Show($"An error occurred while configuring the API key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        private void ConfigureServices(ServiceCollection services)
        {
            // Create configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });
            
            // Add API key manager
            services.AddSingleton<ApiKeyManager>();

            // Add AssemblyAI services
            services.AddAssemblyAIDirectApi(Configuration);
            
            // Add audio extraction service
            services.AddSingleton<IAudioExtractionService, AudioExtractionService>();
            
            // Add transcription repository
            services.AddSingleton<ITranscriptionRepository, TranscriptionRepository>();

            // Add ViewModels
            services.AddTransient<TranscriptionViewModel>();
        }

        /// <summary>
        /// Handles the Exit event of the Application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExitEventArgs"/> instance containing the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up Serilog
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
