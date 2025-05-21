using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using TrialWorld.Core.Models;
using TrialWorld.Presentation.ViewModels;

namespace TrialWorld.Presentation.Dialogs
{
    /// <summary>
    /// Interaction logic for SilenceDetectionOperationsDialog.xaml
    /// </summary>
    public partial class SilenceDetectionOperationsDialog : Window
    {
        private readonly CourtroomMediaPlayerViewModel _courtroomMediaPlayerViewModel; // Renamed field
        private readonly SilenceDetectionViewModel _silenceDetectionViewModel;

        /// <summary>
        /// Initializes a new instance of the SilenceDetectionOperationsDialog class.
        /// </summary>
        /// <param name="courtroomMediaPlayerViewModel">The courtroom media player view model.</param> // Updated param name
        /// <param name="silenceDetectionViewModel">The silence detection view model.</param>
        public SilenceDetectionOperationsDialog(CourtroomMediaPlayerViewModel courtroomMediaPlayerViewModel, SilenceDetectionViewModel silenceDetectionViewModel) // Updated param type
        {
            _courtroomMediaPlayerViewModel = courtroomMediaPlayerViewModel ?? throw new ArgumentNullException(nameof(courtroomMediaPlayerViewModel)); // Updated assignment
            _silenceDetectionViewModel = silenceDetectionViewModel ?? throw new ArgumentNullException(nameof(silenceDetectionViewModel));
            
            InitializeComponent();
            
            // Update button states based on current media state
            UpdateButtonStates();
        }

        /// <summary>
        /// Updates the enabled state of buttons based on current media state.
        /// </summary>
        private void UpdateButtonStates()
        {
            bool canPerformOperations = _courtroomMediaPlayerViewModel.CanDetectSilence; // Updated usage
            
            DetectSilenceButton.IsEnabled = canPerformOperations;
            ExportResultsButton.IsEnabled = _courtroomMediaPlayerViewModel.HasSilenceDetectionResults; // Updated usage
        }

        /// <summary>
        /// Handles the click event for the detect silence button.
        /// </summary>
        private async void DetectSilenceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable buttons during detection
                DetectSilenceButton.IsEnabled = false;
                ExportResultsButton.IsEnabled = false;
                
                // Perform silence detection
                await _courtroomMediaPlayerViewModel.DetectSilenceAsync(); // Updated usage
                
                // Always update button states after detection
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error detecting silence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the export results button.
        /// </summary>
        private void ExportResultsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show save file dialog
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = ".csv",
                    Title = "Export Silence Detection Results",
                    FileName = $"Silence_Detection_Results_{DateTime.Now:yyyyMMdd_HHmmss}"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Export the results
                    _courtroomMediaPlayerViewModel.ExportSilenceDetectionResults(saveFileDialog.FileName); // Updated usage
                    
                    MessageBox.Show(
                        $"Silence detection results exported successfully to:\n{saveFileDialog.FileName}", 
                        "Export Successful", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the settings button.
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SilenceDetectionDialog(_silenceDetectionViewModel);
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                // Settings are updated in _silenceDetectionViewModel
                MessageBox.Show(
                    $"Silence Detection settings saved:\nPadding: {_silenceDetectionViewModel.Padding} seconds\nNoise Ceiling: {_silenceDetectionViewModel.NoiseCeiling} dB",
                    "Settings Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
