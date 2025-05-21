using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Presentation.ViewModels;
using TrialWorld.Presentation.Views;

namespace TrialWorld.Presentation
{
    /// <summary>
    /// Extension methods for registering presentation services with dependency injection
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Adds presentation services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register ViewModels
            services.AddTransient<TranscriptionViewModel>();
            services.AddTransient<SilenceDetectionViewModel>();
            
            // Register Views
            services.AddTransient<TranscriptionView>();
            
            return services;
        }
    }
}
