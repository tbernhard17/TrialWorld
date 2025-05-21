using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Infrastructure.Export
{
    /// <summary>
    /// Extensions for registering export services with dependency injection
    /// </summary>
    public static class ExportServiceRegistration
    {
        /// <summary>
        /// Add export services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddExportServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register export options
            services.Configure<ExportOptions>(configuration.GetSection("Export"));

            /* // Removed: FileDialogService registration belongs in the Presentation layer
            // Register file dialog service
#if WINDOWS
            services.AddTransient<IFileDialogService, WpfFileDialogService>();
#else
            services.AddTransient<IFileDialogService, DefaultFileDialogService>();
#endif
            */

            // Register export service
            services.AddTransient<IExportService, ExportService>();

            return services;
        }
    }
}