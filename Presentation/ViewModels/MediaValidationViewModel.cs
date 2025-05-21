using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using TrialWorld.Contracts;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Presentation.Commands;
using TrialWorld.Contracts.DTOs.MediaValidation;
using DTOs = TrialWorld.Contracts.DTOs.MediaValidation;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel for media validation view
    /// </summary>
    public class MediaValidationViewModel : INotifyPropertyChanged
    {
        // private readonly IMediaValidationService _validationService; // Interface doesn't seem to exist yet
        private readonly ILogger<MediaValidationViewModel> _logger;

        private string? _filePath;
        private bool _isValidating;
        private bool _hasValidated;
        private bool _isValidFile;
        private FileInfo? _fileInfo;
        private double _maxFileSizeMb = 1000;
        private double _minDuration = 5;
        private double _maxDuration = 3600;
        private bool _requiresVideo = true;
        private bool _requiresAudio = true;
        private int _minVideoWidth = 640;
        private int _minVideoHeight = 480;
        private double _minFrameRate = 24;
        private int _minAudioChannels = 2;
        private int _minSampleRate = 44100;

        /// <summary>
        /// Initializes a new instance of the MediaValidationViewModel class
        /// </summary>
        /// <param name="validationService">Media validation service</param>
        /// <param name="logger">Logger</param>
        public MediaValidationViewModel(
            // IMediaValidationService validationService, // Interface doesn't seem to exist yet
            ILogger<MediaValidationViewModel> logger)
        {
            // _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ValidationErrors = new ObservableCollection<ValidationErrorDisplay>();
            MetadataEntries = new ObservableCollection<KeyValuePair<string, string>>();

            BrowseCommand = new RelayCommand(BrowseForFile);
            ValidateCommand = new RelayCommand(async () => await ValidateFileAsync(), CanValidateFile);
        }

        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string? FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(HasFile));

                    // Reset validation state
                    HasValidated = false;
                    IsValidFile = false;
                    ValidationErrors.Clear();
                    MetadataEntries.Clear();
                    FileInfo = null;

                    // Check if file exists and show basic info
                    if (!string.IsNullOrEmpty(value) && File.Exists(value))
                    {
                        var fileInfo = new FileInfo(value);
                        FileInfo = fileInfo;
                    }

                    ((RelayCommand)ValidateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets the filename from the path
        /// </summary>
        public string FileName => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetFileName(FilePath);

        /// <summary>
        /// Gets whether a file is selected
        /// </summary>
        public bool HasFile => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

        /// <summary>
        /// Gets or sets whether validation is in progress
        /// </summary>
        public bool IsValidating
        {
            get => _isValidating;
            set
            {
                if (_isValidating != value)
                {
                    _isValidating = value;
                    OnPropertyChanged();
                    ((RelayCommand)ValidateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether validation has been performed
        /// </summary>
        public bool HasValidated
        {
            get => _hasValidated;
            set
            {
                if (_hasValidated != value)
                {
                    _hasValidated = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the file is valid
        /// </summary>
        public bool IsValidFile
        {
            get => _isValidFile;
            set
            {
                if (_isValidFile != value)
                {
                    _isValidFile = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the file information
        /// </summary>
        public System.IO.FileInfo? FileInfo
        {
            get => _fileInfo;
            set
            {
                if (_fileInfo != value)
                {
                    _fileInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum file size in MB
        /// </summary>
        public double MaxFileSizeMb
        {
            get => _maxFileSizeMb;
            set
            {
                if (_maxFileSizeMb != value)
                {
                    _maxFileSizeMb = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum duration in seconds
        /// </summary>
        public double MinDuration
        {
            get => _minDuration;
            set
            {
                if (_minDuration != value)
                {
                    _minDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum duration in seconds
        /// </summary>
        public double MaxDuration
        {
            get => _maxDuration;
            set
            {
                if (_maxDuration != value)
                {
                    _maxDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether video is required
        /// </summary>
        public bool RequiresVideo
        {
            get => _requiresVideo;
            set
            {
                if (_requiresVideo != value)
                {
                    _requiresVideo = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether audio is required
        /// </summary>
        public bool RequiresAudio
        {
            get => _requiresAudio;
            set
            {
                if (_requiresAudio != value)
                {
                    _requiresAudio = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum video width
        /// </summary>
        public int MinVideoWidth
        {
            get => _minVideoWidth;
            set
            {
                if (_minVideoWidth != value)
                {
                    _minVideoWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum video height
        /// </summary>
        public int MinVideoHeight
        {
            get => _minVideoHeight;
            set
            {
                if (_minVideoHeight != value)
                {
                    _minVideoHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum frame rate
        /// </summary>
        public double MinFrameRate
        {
            get => _minFrameRate;
            set
            {
                if (_minFrameRate != value)
                {
                    _minFrameRate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum audio channels
        /// </summary>
        public int MinAudioChannels
        {
            get => _minAudioChannels;
            set
            {
                if (_minAudioChannels != value)
                {
                    _minAudioChannels = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum sample rate
        /// </summary>
        public int MinSampleRate
        {
            get => _minSampleRate;
            set
            {
                if (_minSampleRate != value)
                {
                    _minSampleRate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of validation errors
        /// </summary>
        public ObservableCollection<ValidationErrorDisplay> ValidationErrors { get; }

        /// <summary>
        /// Gets the collection of metadata entries
        /// </summary>
        public ObservableCollection<KeyValuePair<string, string>> MetadataEntries { get; }

        /// <summary>
        /// Gets the command for browsing for a file
        /// </summary>
        public ICommand BrowseCommand { get; }

        /// <summary>
        /// Gets the command for validating a file
        /// </summary>
        public ICommand ValidateCommand { get; }

        /// <summary>
        /// Opens a file dialog to browse for a media file
        /// </summary>
        private void BrowseForFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.mp3;*.wav;*.aac;*.m4a;*.flac|All Files|*.*",
                Title = "Select Media File"
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Determines whether a file can be validated
        /// </summary>
        /// <returns>True if a file can be validated, otherwise false</returns>
        private bool CanValidateFile()
        {
            return !IsValidating && HasFile;
        }

        /// <summary>
        /// Validates the selected file
        /// </summary>
        private async Task ValidateFileAsync()
        {
            if (!HasFile || IsValidating)
                return;

            try
            {
                IsValidating = true;
                ValidationErrors.Clear();
                MetadataEntries.Clear();

                _logger.LogInformation("Validating file: {FilePath}", FilePath);

                // Create validation requirements based on UI settings
                var requirements = new MediaRequirementsDTO
                {
                    MaxFileSizeMb = (int)MaxFileSizeMb,
                    MinDuration = TimeSpan.FromSeconds(MinDuration),
                    MaxDuration = TimeSpan.FromSeconds(MaxDuration),
                    RequiresVideo = RequiresVideo,
                    RequiresAudio = RequiresAudio
                };

                if (RequiresVideo)
                {
                    requirements.VideoRequirements = new DTOs.VideoRequirementsDTO
                    {
                        MinimumResolutionWidth = MinVideoWidth,
                        MinimumResolutionHeight = MinVideoHeight,
                        MinimumFramerate = MinFrameRate,
                        MinimumBitrateKbps = 0, // Set to 0 or use a property if available
                        RequireColor = false,   // Set to false or use a property if available
                        RequireStableImage = false // Set to false or use a property if available
                    };
                }

                if (RequiresAudio)
                {
                    requirements.AudioRequirements = new DTOs.AudioRequirementsDTO
                    {
                        MinimumSampleRateHz = MinSampleRate,
                        MinimumBitrateKbps = 0, // Set to 0 or use a property if available
                        MinimumLoudnessDb = 0.0, // Set to 0.0 or use a property if available
                        MaximumNoiseLevelDb = 0.0, // Set to 0.0 or use a property if available
                        RequireVocalPresence = false // Set to false or use a property if available
                    };
                }

                // Create validation request
                var request = new DTOs.MediaValidationRequestDTO
                {
                    FilePath = FilePath ?? string.Empty,
                    Requirements = requirements
                };

                // Validate the file
                // var result = await _validationService.ValidateMediaAsync(request); // Interface/method doesn't seem to exist yet

                // --- Placeholder result for testing UI without service ---
                var result = new MediaValidationResultDTO
                {
                    IsValid = false, // Assume invalid for now
                    Errors = new List<ValidationErrorDTO> { new ValidationErrorDTO("Placeholder", "Validation service not implemented.") },
                    Metadata = new Dictionary<string, object>()
                };
                await Task.Delay(500); // Simulate async work
                // --- End Placeholder ---

                // Update UI
                HasValidated = true;
                IsValidFile = result.IsValid;

                // Add errors to the list
                if (!result.IsValid && result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors)
                    {
                        ValidationErrors.Add(new ValidationErrorDisplay(error.Code, error.Message));
                    }
                }

                // Add metadata to the list
                if (result.Metadata.Count > 0)
                {
                    foreach (var entry in result.Metadata)
                    {
                        MetadataEntries.Add(new KeyValuePair<string, string>(entry.Key, entry.Value?.ToString() ?? ""));
                    }
                }

                _logger.LogInformation("Validation completed. IsValid: {IsValid}, Error count: {ErrorCount}",
                    result.IsValid, result.Errors.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file: {FilePath}", FilePath);

                HasValidated = true;
                IsValidFile = false;
                ValidationErrors.Add(new ValidationErrorDisplay("Error", $"An unexpected error occurred: {ex.Message}"));

                MessageBox.Show(
                    $"An error occurred while validating the file: {ex.Message}",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsValidating = false;
            }
        }

        /// <summary>
        /// Formats file size in a human-readable format
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Formatted file size</returns>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ValidationErrorDisplay
    {
        /// <summary>
        /// Initializes a new instance of the ValidationErrorDisplay class
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        public ValidationErrorDisplay(string code, string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Gets the error code
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Message { get; }
    }
}