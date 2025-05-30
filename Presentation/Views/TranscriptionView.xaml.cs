using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Presentation.ViewModels;
using TrialWorld.Presentation.Commands;
using Microsoft.Extensions.Logging;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace TrialWorld.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TranscriptionView.xaml
    /// </summary>
    public partial class TranscriptionView : UserControl
    {
        private TranscriptionViewModel? _viewModel;

        public TranscriptionView()
        {
            InitializeComponent();
            
            // The ViewModel should be injected via DI in the actual app
            // This is just for design-time support
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignTimeTranscriptionViewModel();
            }
            
            // Wait for DataContext to be set before accessing it
            this.Loaded += (s, e) =>
            {
                try
                {
                    _viewModel = DataContext as TranscriptionViewModel;
                    if (_viewModel == null)
                    {
                        // Create a default view model if none is provided
                        var service = new DesignTimeTranscriptionService();
                        var logger = new DesignTimeLogger<TranscriptionViewModel>();
                        DataContext = new TranscriptionViewModel(service, logger);
                        _viewModel = (TranscriptionViewModel)DataContext;
                    }
                    
                    // Add commands that need access to the view
                    if (_viewModel != null)
                    {
                        _viewModel.BrowseMediaCommand = new RelayCommand(() => ExecuteBrowseMediaCommand());
                        _viewModel.OpenTranscriptionCommand = new ParameterizedRelayCommand(param => ExecuteOpenTranscriptionCommand(param));
                        _viewModel.RetryTranscriptionCommand = new ParameterizedRelayCommand(param => ExecuteRetryTranscriptionCommand(param));
                        _viewModel.ClearErrorCommand = new RelayCommand(() => ExecuteClearErrorCommand());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error initializing view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void ExecuteBrowseMediaCommand(object? parameter = null)
        {
            if (_viewModel == null) return;
            
            var dialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.mp3;*.wav;*.m4a|All Files|*.*",
                Title = "Select Media File"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.MediaPath = dialog.FileName;
            }
        }

        private void ExecuteOpenTranscriptionCommand(object? parameter)
        {
            if (_viewModel == null) return;
            
            if (parameter is TranscriptionQueueItem item)
            {
                // Open the transcript file if it exists
                if (!string.IsNullOrEmpty(item.OutputPath) && File.Exists(item.OutputPath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = item.OutputPath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        _viewModel.ErrorMessage = $"Error opening transcript: {ex.Message}";
                    }
                }
                else
                {
                    _viewModel.ErrorMessage = "Transcript file not found";
                }
            }
        }

        private void ExecuteRetryTranscriptionCommand(object? parameter)
        {
            if (_viewModel == null) return;
            
            if (parameter is TranscriptionQueueItem item)
            {
                _viewModel.MediaPath = item.MediaPath;
                _viewModel.TranscribeCommand?.Execute(null);
            }
        }

        private void ExecuteClearErrorCommand(object? parameter = null)
        {
            if (_viewModel == null) return;
            
            _viewModel.ErrorMessage = string.Empty;
        }
    }

    /// <summary>
    /// Design-time view model for the TranscriptionView
    /// </summary>
    internal class DesignTimeTranscriptionViewModel : TranscriptionViewModel
    {
        public DesignTimeTranscriptionViewModel() 
            : base(new DesignTimeTranscriptionService(), new DesignTimeLogger<TranscriptionViewModel>())
        {
            // Add some sample data for design-time preview
            MediaPath = @"C:\Sample\Video.mp4";
            StatusMessage = "Processing transcription...";
            TranscriptionProgress = 45;
            
            QueueItems.Add(new TranscriptionQueueItem
            {
                MediaPath = @"C:\Sample\Video1.mp4",
                TranscriptionId = "sample-id-1",
                Status = Core.Models.Transcription.TranscriptionStatus.Completed,
                Progress = 100,
                SubmittedAt = DateTime.Now.AddHours(-2),
                LastUpdated = DateTime.Now.AddHours(-1),
                OutputPath = @"C:\Sample\Video1.json"
            });
            
            QueueItems.Add(new TranscriptionQueueItem
            {
                MediaPath = @"C:\Sample\Video2.mp4",
                TranscriptionId = "sample-id-2",
                Status = Core.Models.Transcription.TranscriptionStatus.Processing,
                Progress = 60,
                SubmittedAt = DateTime.Now.AddMinutes(-30),
                LastUpdated = DateTime.Now.AddMinutes(-5)
            });
            
            QueueItems.Add(new TranscriptionQueueItem
            {
                MediaPath = @"C:\Sample\Video3.mp4",
                TranscriptionId = "sample-id-3",
                Status = Core.Models.Transcription.TranscriptionStatus.Failed,
                Progress = 20,
                SubmittedAt = DateTime.Now.AddHours(-1),
                LastUpdated = DateTime.Now.AddMinutes(-50)
            });
        }
    }

    /// <summary>
    /// Design-time implementation of ITranscriptionService
    /// </summary>
    internal class DesignTimeTranscriptionService : Core.Interfaces.ITranscriptionService
    {
        public Task<bool> CancelTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DownloadTranscriptionFileAsync(string transcriptionId, string outputPath, IProgress<Core.Models.Transcription.TranscriptionProgressUpdate>? progress = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
        
        public Task<Core.Models.Transcription.TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            Core.Models.Transcription.TranscriptionConfig config,
            IProgress<Core.Models.Transcription.TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new Core.Models.Transcription.TranscriptionResult
            {
                TranscriptId = "design-time-id",
                Status = Core.Models.Transcription.TranscriptionStatus.Processing
            });
        }
        
        public Task<Core.Models.Transcription.TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<Core.Models.Transcription.TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            return TranscribeWithProgressAsync(
                filePath, 
                new Core.Models.Transcription.TranscriptionConfig(), 
                progress, 
                cancellationToken);
        }

        public Task<Core.Models.Transcription.TranscriptionStatus> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Core.Models.Transcription.TranscriptionStatus.Processing);
        }

        public Task<Core.Models.Transcription.TranscriptionResult> TranscribeAsync(string filePath, Core.Models.Transcription.TranscriptionConfig config, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Core.Models.Transcription.TranscriptionResult
            {
                TranscriptId = "design-time-id",
                Status = Core.Models.Transcription.TranscriptionStatus.Processing
            });
        }
        
        public Task<Core.Models.Transcription.TranscriptionResult> TranscribeAsync(string filePath, CancellationToken cancellationToken)
        {
            return TranscribeAsync(filePath, new Core.Models.Transcription.TranscriptionConfig(), cancellationToken);
        }
    }

    /// <summary>
    /// Design-time implementation of ILogger
    /// </summary>
    internal class DesignTimeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        
        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }
    }
}
