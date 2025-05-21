using System.Windows;
using TrialWorld.Presentation.ViewModels;

namespace TrialWorld.Presentation.Dialogs
{
    /// <summary>
    /// Dialog for configuring silence detection settings
    /// </summary>
    public partial class SilenceDetectionDialog : Window
    {
        /// <summary>
        /// The view model containing the silence detection settings
        /// </summary>
        public SilenceDetectionViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the SilenceDetectionDialog
        /// </summary>
        /// <param name="viewModel">The view model with the current settings</param>
        public SilenceDetectionDialog(SilenceDetectionViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? new SilenceDetectionViewModel(); // Prevent null reference
            DataContext = ViewModel;
        }

        /// <summary>
        /// Handles the Save button click
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate padding (0-30 seconds)
            if (ViewModel.Padding < 0 || ViewModel.Padding > 30)
            {
                MessageBox.Show("Padding must be between 0 and 30 seconds.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Validate noise ceiling (-60 to -10 dB)
            if (ViewModel.NoiseCeiling < -60 || ViewModel.NoiseCeiling > -10)
            {
                MessageBox.Show("Noise ceiling must be between -60 and -10 dB.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Set dialog result to true and close the dialog
            DialogResult = true;
            Close();
        }

        // Cancel button uses IsCancel="True" in XAML which automatically handles cancellation
    }
}