using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Presentation.ViewModels;

namespace TrialWorld.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MediaValidationView.xaml
    /// </summary>
    public partial class MediaValidationView : Window
    {
        /// <summary>
        /// Initializes a new instance of the MediaValidationView class
        /// </summary>
        public MediaValidationView()
        {
            InitializeComponent();

            // Get the service provider (replace with your DI setup)
            var serviceProvider = ((App)System.Windows.Application.Current).ServiceProvider
                ?? throw new InvalidOperationException("ServiceProvider is not initialized.");

            // Create ViewModel
            var logger = serviceProvider.GetRequiredService<ILogger<MediaValidationViewModel>>();

            // Set DataContext
            DataContext = new MediaValidationViewModel(logger);
        }
    }
}