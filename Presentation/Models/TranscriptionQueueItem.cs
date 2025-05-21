using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Presentation.Models
{
    /// <summary>
    /// Represents an item in the transcription queue with metadata for tracking and validation
    /// </summary>
    public enum TranscriptionStage
    {
        None,
        SilenceDetection,
        AudioExtraction,
        Upload,
        Transcription,
        Download,
        Complete,
        Failed
    }

    public class TranscriptionQueueItem : INotifyPropertyChanged
    {
        private string _status = string.Empty;
        private double _silenceDetectionProgress;
        private double _audioExtractionProgress;
        private double _uploadProgress;
        private double _transcriptionProgress;
        private double _downloadProgress;
        // Field renamed to follow consistent naming convention
        // private double _transcribeProgress;
        private double _processProgress;
        private TranscriptionStage _currentStage = TranscriptionStage.None;
        private string _filePath = string.Empty;
        private string _fileName = string.Empty;
        private string _fileHash = string.Empty;
        private string _transcriptionId = string.Empty;
        private DateTime _lastAttemptTime = DateTime.MinValue;
        private bool _isVerified = false;
        private string _outputFilePath = string.Empty;
        private int _attemptCount = 0;
        private TranscriptionPhase _currentPhase = TranscriptionPhase.Queued;
        private double _overallProgress;

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                    if (!string.IsNullOrEmpty(value))
                    {
                        FileName = System.IO.Path.GetFileName(value);
                    }
                }
            }
        }
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }
        public string StatusDisplay
        {
            get
            {
                return TrialWorld.Core.Utilities.TranscriptionStatusMapper.ToDisplayString(
                    _status,
                    _silenceDetectionProgress,
                    _uploadProgress,
                    _transcriptionProgress
                );
            }
        }

        /// <summary>
        /// Unified progress value (0-100) for this transcription item, using centralized calculation logic.
        /// </summary>
        public double Progress
        {
            get
            {
                return TrialWorld.Core.Utilities.TranscriptionProgressCalculator.CalculateProgress(_status, _transcriptionProgress);
            }
        }
        public double SilenceDetectionProgress
        {
            get => _silenceDetectionProgress;
            set
            {
                if (_silenceDetectionProgress != value)
                {
                    _silenceDetectionProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double AudioExtractionProgress
        {
            get => _audioExtractionProgress;
            set
            {
                if (_audioExtractionProgress != value)
                {
                    _audioExtractionProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double UploadProgress
        {
            get => _uploadProgress;
            set
            {
                if (_uploadProgress != value)
                {
                    _uploadProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double TranscribeProgress
        {
            get => _transcriptionProgress;
            set
            {
                if (_transcriptionProgress != value)
                {
                    _transcriptionProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ProcessProgress
        {
            get => _processProgress;
            set
            {
                if (_processProgress != value)
                {
                    _processProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        // Legacy property for compatibility (if needed)
        public double TranscriptionProgress
        {
            get => TranscribeProgress;
            set => TranscribeProgress = value;
        }
        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                if (_downloadProgress != value)
                {
                    _downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }


        public TranscriptionStage CurrentStage
        {
            get => _currentStage;
            set
            {
                if (_currentStage != value)
                {
                    _currentStage = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Returns a unified overall progress value (0-100) based on the current stage and per-stage progress.
        /// Can also be explicitly set when needed.
        /// </summary>
        public double OverallProgress
        {
            get
            {
                // If explicitly set, return the stored value
                if (_overallProgress > 0)
                {
                    return _overallProgress;
                }
                
                // Otherwise calculate based on current stage
                switch (CurrentStage)
                {
                    case TranscriptionStage.SilenceDetection: return SilenceDetectionProgress * 0.2;
                    case TranscriptionStage.AudioExtraction: return 20 + AudioExtractionProgress * 0.2;
                    case TranscriptionStage.Upload: return 40 + UploadProgress * 0.2;
                    case TranscriptionStage.Transcription: return 60 + TranscriptionProgress * 0.3;
                    case TranscriptionStage.Download: return 90 + DownloadProgress * 0.1;
                    case TranscriptionStage.Complete: return 100;
                    case TranscriptionStage.Failed: return 0;
                    default: return 0;
                }
            }
            set
            {
                if (_overallProgress != value)
                {
                    _overallProgress = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileHash
        {
            get => _fileHash;
            set
            {
                if (_fileHash != value)
                {
                    _fileHash = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TranscriptionId
        {
            get => _transcriptionId;
            set
            {
                if (_transcriptionId != value)
                {
                    _transcriptionId = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LastAttemptTime
        {
            get => _lastAttemptTime;
            set
            {
                if (_lastAttemptTime != value)
                {
                    _lastAttemptTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsVerified
        {
            get => _isVerified;
            set
            {
                if (_isVerified != value)
                {
                    _isVerified = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OutputFilePath
        {
            get => _outputFilePath;
            set
            {
                if (_outputFilePath != value)
                {
                    _outputFilePath = value;
                    OnPropertyChanged();
                }
            }
        }
        public int AttemptCount
        {
            get => _attemptCount;
            set
            {
                if (_attemptCount != value)
                {
                    _attemptCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public TranscriptionPhase CurrentPhase
        {
            get => _currentPhase;
            set
            {
                if (_currentPhase != value)
                {
                    _currentPhase = value;
                    Status = value.ToString();
                    OnPropertyChanged();
                }
            }
        }
        private void UpdateProcessProgress()
        {
            double weightedProgress =
                (_silenceDetectionProgress * 0.15) +
                (_uploadProgress * 0.25) +
                (_transcriptionProgress * 0.60);
            ProcessProgress = weightedProgress;
        }
        public bool CanBeProcessed()
        {
            return CurrentPhase == TranscriptionPhase.Queued ||
                   (CurrentPhase == TranscriptionPhase.Failed && AttemptCount < 3);
        }
        public bool IsQueuedOrProcessing()
        {
            return CurrentPhase == TranscriptionPhase.Queued ||
                   CurrentPhase == TranscriptionPhase.SilenceDetection ||
                   CurrentPhase == TranscriptionPhase.Uploading ||
                   CurrentPhase == TranscriptionPhase.Submitted ||
                   CurrentPhase == TranscriptionPhase.Processing;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
