using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Presentation.Interfaces;
using TrialWorld.Core.Services;
using TrialWorld.Presentation.ViewModels;
using TrialWorld.Presentation.ViewModels.Search;
using TrialWorld.Presentation.Services;

namespace TrialWorld.Presentation
{
    public static class PresentationServiceRegistration
    {
        public static IServiceCollection AddPresentationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register view models
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<CourtroomMediaPlayerViewModel>();
            services.AddTransient<MediaTranscriptViewModel>();
            // Removed DashboardViewModel registration (type does not exist)
            // Removed EnhancementViewModel registration (type does not exist)
            services.AddTransient<AnalysisViewModel>();
            services.AddTransient<SearchViewModel>();

            // Register UI services using ServiceLifetimeManager
            ServiceLifetimeManager.RegisterService<TrialWorld.Presentation.Interfaces.IDialogService, TrialWorld.Presentation.Services.DialogService>(services);
            ServiceLifetimeManager.RegisterService<INavigationService, NavigationService>(services);
            ServiceLifetimeManager.RegisterService<INotificationService, NotificationService>(services);
            ServiceLifetimeManager.RegisterService<IFileDialogService, Presentation.Services.FileDialogService>(services);

            // Register UI-specific services
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IWindowManager, WindowManager>();

            return services;
        }
    }
}