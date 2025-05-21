using TrialWorld.Presentation.Dialogs;
using TrialWorld.Presentation.ViewModels;
using TrialWorld.Presentation.Interfaces;
using TrialWorld.Presentation.Models;
using ModelTranscriptionQueueItem = TrialWorld.Presentation.Models.TranscriptionQueueItem;
using ViewModelTranscriptionQueueItem = TrialWorld.Presentation.ViewModels.TranscriptionQueueItem;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Win32;
using TrialWorld.Core.Interfaces;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Models.Configuration;
using System.IO;

namespace TrialWorld.Presentation
{
    public partial class MainWindow : Window
    {
        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private readonly ILogger<MainWindow> _logger;
        private readonly MainWindowViewModel _viewModel = null!;
        private readonly INotificationService _notificationService;
        private readonly Views.TranscriptionView _transcriptionView;

        public MainWindow(MainWindowViewModel viewModel, ILogger<MainWindow> logger, INotificationService notificationService, Views.TranscriptionView transcriptionView)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MainWindow constructor called. viewModel is null: {viewModel == null}");
            
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));
            if (viewModel is null)
                throw new ArgumentNullException(nameof(viewModel));
            if (notificationService is null)
                throw new ArgumentNullException(nameof(notificationService));
            if (transcriptionView is null)
                throw new ArgumentNullException(nameof(transcriptionView));
                
            logger.LogInformation("[DEBUG] MainWindow constructor called. viewModel is null: {isNull}", viewModel == null);

            InitializeComponent();
            DataContext = viewModel;
            _logger = logger;
            _viewModel = viewModel;
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _transcriptionView = transcriptionView;
            
            // this.KeyDown += MainWindow_KeyDown; // Re-evaluate if global keybindings are needed
        }

        void CheckFFmpegAvailability()
        {
            try
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.StatusText = "Checking FFmpeg availability...";
                    vm.StatusText = $"FFmpeg status unknown";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error detecting FFmpeg: {ex.Message}\n\nThe application may not function correctly without FFmpeg installed.",
                    "FFmpeg Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.StatusText = "FFmpeg not detected - functionality limited";
                }
                _logger.LogError(ex, "Error detecting FFmpeg");
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logger.LogInformation("Application shutting down");
        }

        void ResourceMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _currentProcess.Refresh();
                double memoryMB = _currentProcess.WorkingSet64 / (1024.0 * 1024.0);
                _logger.LogInformation("CPU: {CpuTime} ms, RAM: {MemoryMB} MB",
                    _currentProcess.TotalProcessorTime.TotalMilliseconds, memoryMB);
            }
            catch
            {
                // Ignore
            }
        }
        
        /// <summary>
        /// Handles the TabControl selection changed event
        /// </summary>
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
                {
                    _logger.LogInformation("Tab selected: {TabHeader}", (selectedTab.Header as string) ?? "Unknown");
                    
                    // Handle tab selection based on the header
                    string? header = selectedTab.Header as string;
                    
                    if (header == "Transcript" && _viewModel.MediaTranscriptViewModel != null)
                    {
                        // If media is loaded in the player, set it in the MediaTranscriptViewModel
                        if (_viewModel.CourtroomMediaPlayerViewModel != null && 
                            !string.IsNullOrEmpty(_viewModel.CourtroomMediaPlayerViewModel.CurrentMediaPath))
                        {
                            _viewModel.MediaTranscriptViewModel.MediaPath = _viewModel.CourtroomMediaPlayerViewModel.CurrentMediaPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling tab selection change");
            }
        }
        

        



        
        string GetApplicationVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Version;
                return version != null
                    ? $"{version.Major}.{version.Minor}.{version.Build}"
                    : "1.0.0";
            }
            catch
            {
                return "1.0.0";
            }
        }

        #region Menu Event Handlers
        void OpenMedia_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.mp3;*.wav;*.aac|All Files|*.*",
                Title = "Select Media File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (_viewModel.CourtroomMediaPlayerViewModel != null)
                {
                    _viewModel.CourtroomMediaPlayerViewModel.CurrentMediaPath = openFileDialog.FileName;
                    // TODO: Consider adding a LoadMedia or Play command to CourtroomMediaPlayerViewModel
                    // For example: _viewModel.CourtroomMediaPlayerViewModel.LoadMediaCommand.Execute(openFileDialog.FileName);
                }
            }
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings dialog not yet implemented.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ManageModels_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Model management not yet implemented.", "Manage Models", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void About_Click(object sender, RoutedEventArgs e)
        {
            string message = "Trial World AI - Media Analysis Tool\n\n"
                + $"Version: {GetApplicationVersion()}\n"
                + " 2023 Trial World AI\n\n"
                + "A powerful media analysis application that leverages AI \n"
                + "to provide face detection, transcription, content analysis, \n"
                + "and more for your media files.";
            MessageBox.Show(message, "About Trial World AI", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        public void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenMedia_Click(sender, e);
        }

        void Documentation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Documentation not yet available.", "Documentation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Global key bindings can be handled here if they don't conflict with the player.
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        public void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            
            MenuItem extractedAudioItem = new MenuItem() { Header = "Extracted Audio" };
            extractedAudioItem.Click += PickExtractedAudioFolder_Click;
            menu.Items.Add(extractedAudioItem);
            
            MenuItem transcriptsItem = new MenuItem() { Header = "Transcripts" };
            transcriptsItem.Click += PickTranscriptsFolder_Click;
            menu.Items.Add(transcriptsItem);
            
            MenuItem databaseItem = new MenuItem() { Header = "Transcription Database" };
            databaseItem.Click += PickTranscriptionDatabaseFolder_Click;
            menu.Items.Add(databaseItem);
            
            menu.Items.Add(new Separator());
            
            MenuItem generalSettingsItem = new MenuItem() { Header = "Settings" };
            generalSettingsItem.Click += Settings_Click; 
            menu.Items.Add(generalSettingsItem);
            
            menu.PlacementTarget = (Button)sender;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }
        
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("SearchButton_Click: Functionality to show/focus search panel needs review after UI consolidation.");
            // RightPanel.Visibility = System.Windows.Visibility.Visible; // Old XAML element
            // SearchTextBox.Focus(); // Old XAML element
        }
        
        private void SearchPanelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Search button clicked");
                
                // Get the search text from the UI
                string searchText = string.Empty;
                if (SearchTextBox != null)
                {
                    searchText = SearchTextBox.Text?.Trim() ?? string.Empty;
                }
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    MessageBox.Show("Please enter a search term", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // Perform search using the view model
                _viewModel.SearchResults.Clear();
                
                // Check if we have a transcript to search in
                if (_viewModel.MediaTranscriptViewModel != null && _viewModel.MediaTranscriptViewModel.HasTranscript)
                {
                    // Search in the current transcript
                    _logger.LogInformation("Searching for '{SearchText}' in transcript", searchText);
                    
                    // Update status
                    _viewModel.StatusText = $"Searching for '{searchText}'...";
                    
                    // Perform the search asynchronously
                    PerformSearchAsync().ConfigureAwait(false);
                }
                else
                {
                    MessageBox.Show("No transcript available to search", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search");
                MessageBox.Show($"Error performing search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task PerformSearchAsync()
        {
            if (DataContext is MainWindowViewModel vm)
            {
                string searchText = ""; // Was SearchTextBox.Text
                bool includeWords = true; // Was WordsCheckBox.IsChecked
                bool includeSentiment = true; // Was SentimentCheckBox.IsChecked
                bool includeHighlights = true; // Was HighlightsCheckBox.IsChecked
                bool includeChapters = true; // Was ChaptersCheckBox.IsChecked
                string sentimentFilter = "All"; // Was radio button group
                
                await vm.SearchAsync(searchText, includeWords, includeSentiment, includeHighlights, includeChapters, sentimentFilter);
                _logger.LogInformation("PerformSearchAsync needs review after UI consolidation and removal of direct XAML element access.");
            }
        }
        
        private void JumpToTimestamp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TrialWorld.Presentation.Models.SearchResultItem resultItem)
                {
                    _logger.LogInformation("Jumping to timestamp: {Timestamp}", resultItem.FormattedTime);
                    
                    // Use the CourtroomMediaPlayerViewModel to seek to the position
                    if (_viewModel.CourtroomMediaPlayerViewModel != null)
                    {
                        // Pass the timestamp in milliseconds directly to SeekToPosition
                        // which expects an int parameter
                        _viewModel.CourtroomMediaPlayerViewModel.SeekToPosition(resultItem.TimestampMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error jumping to timestamp");
                MessageBox.Show($"Error jumping to timestamp: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddFileMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.AddFileCommand.Execute(null);
            }
        }

        private void AddFolderMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.AddFolderCommand.Execute(null);
            }
        }

        private void CancelAllMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CancelAllCommand.Execute(null);
            }
        }

        private void ClearQueueMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.ClearQueueCommand.Execute(null);
            }
        }

        private void ProcessAll_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.ProcessAllCommand.Execute(null);
            }
        }

        private void CancelAll_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CancelAllCommand.Execute(null);
            }
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.ClearQueueCommand.Execute(null);
            }
        }

        private void StopSingle_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && sender is FrameworkElement element && element.Tag is Models.TranscriptionQueueItem item)
            {
                _viewModel.StopSingleCommand.Execute(item);
            }
        }

        private void RemoveSingle_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && sender is FrameworkElement element && element.Tag is Models.TranscriptionQueueItem item)
            {
                _viewModel.RemoveSingleCommand.Execute(item);
            }
        }
        
        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog to select media files
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.mp3;*.wav;*.aac|All Files|*.*",
                Title = "Select Media File for Transcription",
                Multiselect = true
            };
            
            // Get the position of the left panel to position the dialog
            var leftPanel = (UIElement)this.FindName("leftPanel");
            Point leftPanelPosition = new Point(10, 10); // Default position
            
            if (leftPanel != null)
            {
                // Get the position of the left panel relative to the window
                leftPanelPosition = leftPanel.TranslatePoint(new Point(0, 0), this);
            }
            
            // Set the dialog's initial position using Win32 API
            // This requires P/Invoke which is beyond the scope of this fix
            // Instead we'll use the standard dialog which will open in the center
            
            if (openFileDialog.ShowDialog() == true)
            {
                // Add each selected file to the transcription queue
                foreach (string filename in openFileDialog.FileNames)
                {
                    if (_viewModel != null)
                    {
                        _viewModel.AddFileCommand.Execute(filename);
                    }
                }
            }
        }

        void PickExtractedAudioFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _logger.LogInformation("Extracted Audio folder selected: {Path}", dlg.SelectedPath);
                _notificationService.ShowSuccess($"Extracted Audio folder will be set to: {dlg.SelectedPath}", "Setting Updated");
            }
        }

        void PickTranscriptsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _logger.LogInformation("Transcripts folder selected: {Path}", dlg.SelectedPath);
                _notificationService.ShowSuccess($"Transcripts folder will be set to: {dlg.SelectedPath}", "Setting Updated");
            }
        }

        void PickTranscriptionDatabaseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _logger.LogInformation("Transcription Database folder selected: {Path}", dlg.SelectedPath);
                _notificationService.ShowSuccess($"Transcription Database folder will be set to: {dlg.SelectedPath}", "Setting Updated");
            }
        }

        // MainWindow-specific silence detection methods and related fields are removed.
        // This functionality is now centralized in CourtroomMediaPlayer and its CourtroomMediaPlayerViewModel.
        
        // SelectTranscriptionModel method removed as it was tied to the old silence UI/TranscriptionModelViewModel
    }
}