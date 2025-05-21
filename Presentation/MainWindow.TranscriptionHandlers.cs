using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace TrialWorld.Presentation
{
    /// <summary>
    /// Partial class containing transcription-related handlers for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Handles the Transcription tab selection
        /// </summary>
        private void TranscriptionTab_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Transcription tab selected");
                
                // If media is loaded in the player, set it in the MediaTranscriptViewModel
                if (_viewModel.CourtroomMediaPlayerViewModel != null && 
                    !string.IsNullOrEmpty(_viewModel.CourtroomMediaPlayerViewModel.CurrentMediaPath) &&
                    _viewModel.MediaTranscriptViewModel != null)
                {
                    _viewModel.MediaTranscriptViewModel.MediaPath = _viewModel.CourtroomMediaPlayerViewModel.CurrentMediaPath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling transcription tab selection");
                MessageBox.Show($"Error showing transcript: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
