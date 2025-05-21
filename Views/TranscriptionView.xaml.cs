using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TrialWorld.AssemblyAIDiagnostic.ViewModels;

namespace TrialWorld.AssemblyAIDiagnostic.Views
{
    /// <summary>
    /// Interaction logic for TranscriptionView.xaml
    /// </summary>
    public partial class TranscriptionView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionView"/> class.
        /// </summary>
        public TranscriptionView()
        {
            InitializeComponent();
            
            // Get the ViewModel from the service provider
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            var viewModel = serviceProvider.GetRequiredService<TranscriptionViewModel>();
            
            // Set the DataContext
            DataContext = viewModel;
        }
    }
}
