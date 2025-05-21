using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    /// <summary>
    /// Provides centralized state management for various application processes
    /// Stores current state of lengthy operations and notifies subscribers of changes
    /// </summary>
    public class CentralStateService : ICentralStateService, INotifyPropertyChanged
    {
        private readonly TrialWorld.Core.Common.Interfaces.IEventAggregator _eventAggregator;
        private readonly ILoggingService _logger;
        
        private bool _isPlaying;
        private TimeSpan _currentPosition;
        private TimeSpan _duration;
        private float _volume = 1.0f;
        private bool _isTranscribing;
        private double _transcriptionProgress;
        private string _transcriptionStatus = "Ready";
        private string _transcriptionText = string.Empty;
        private bool _isEnhancing;
        private double _enhancementProgress;
        private string _enhancementStatus = "Ready";
        private bool _hasAppliedEnhancements;
        private string _currentMediaPath = string.Empty;
        private string _currentMediaName = string.Empty;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StateChanged;
        
        /// <summary>
        /// Creates a new instance of the CentralStateService
        /// </summary>
        /// <param name="eventAggregator">Event aggregator for publishing state changes</param>
        /// <param name="logger">Logging service</param>
        public CentralStateService(TrialWorld.Core.Common.Interfaces.IEventAggregator eventAggregator, ILoggingService logger)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInfo("Central state service initialized");
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public float Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        public bool IsTranscribing => _isTranscribing;

        public double TranscriptionProgress => _transcriptionProgress;

        public string TranscriptionStatus => _transcriptionStatus;

        public string TranscriptionText
        {
            get => _transcriptionText;
            set => SetProperty(ref _transcriptionText, value);
        }

        public bool IsEnhancing => _isEnhancing;

        public double EnhancementProgress => _enhancementProgress;
        
        public string EnhancementStatus => _enhancementStatus;

        public bool HasAppliedEnhancements
        {
            get => _hasAppliedEnhancements;
            set => SetProperty(ref _hasAppliedEnhancements, value);
        }

        public string CurrentMediaPath
        {
            get => _currentMediaPath;
            set => SetProperty(ref _currentMediaPath, value);
        }

        public string CurrentMediaName
        {
            get => _currentMediaName;
            set => SetProperty(ref _currentMediaName, value);
        }

        public void UpdateMediaState(string path, string name, TimeSpan duration)
        {
            CurrentMediaPath = path;
            CurrentMediaName = name;
            Duration = duration;
            CurrentPosition = TimeSpan.Zero;
            IsPlaying = false;
            HasAppliedEnhancements = false;
            
            _logger.LogDebug($"Media state updated: {name}");
        }

        public void UpdateTranscriptionState(bool isActive, double progress, string status)
        {
            SetProperty(ref _isTranscribing, isActive);
            SetProperty(ref _transcriptionProgress, progress);
            var nonNullStatus = status ?? "No status";
            SetProperty<string>(ref _transcriptionStatus, nonNullStatus);
            
            _logger.LogDebug($"Transcription state updated: {progress}% - {nonNullStatus}");
            
            StateChanged?.Invoke(this, EventArgs.Empty);
            _eventAggregator.Publish(new StateChangedMessage
            {
                StateType = StateType.TranscriptionState,
                Status = nonNullStatus,
                Progress = progress
            });
        }
        
        public void UpdateEnhancementState(bool isActive, double progress, string status)
        {
            SetProperty(ref _isEnhancing, isActive);
            SetProperty(ref _enhancementProgress, progress);
            var nonNullStatus = status ?? "No status";
            SetProperty<string>(ref _enhancementStatus, nonNullStatus);
            
            _logger.LogDebug($"Enhancement state updated: {progress}% - {nonNullStatus}");
            
            StateChanged?.Invoke(this, EventArgs.Empty);
            _eventAggregator.Publish(new StateChangedMessage
            {
                StateType = StateType.EnhancementState,
                Status = nonNullStatus,
                Progress = progress
            });
        }

        public void ResetAllStates()
        {
            IsPlaying = false;
            CurrentPosition = TimeSpan.Zero;
            Duration = TimeSpan.Zero;
            SetProperty(ref _isTranscribing, false);
            SetProperty(ref _transcriptionProgress, 0);
            SetProperty(ref _transcriptionStatus, "Ready");
            TranscriptionText = string.Empty;
            SetProperty(ref _isEnhancing, false);
            SetProperty(ref _enhancementProgress, 0);
            SetProperty(ref _enhancementStatus, "Ready");
            HasAppliedEnhancements = false;
            CurrentMediaPath = string.Empty;
            CurrentMediaName = string.Empty;
            
            _logger.LogInfo("All states reset to default");
            
            StateChanged?.Invoke(this, EventArgs.Empty);
            _eventAggregator.Publish(new StateChangedMessage
            {
                StateType = StateType.AllStates
            });
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <summary>
    /// Message indicating a state change
    /// </summary>
    public class StateChangedMessage
    {
        public StateType StateType { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public DateTime Timestamp { get; } = DateTime.Now;
    }
    
    /// <summary>
    /// Types of state that can change
    /// </summary>
    public enum StateType
    {
        EnhancementState,
        TranscriptionState,
        MediaState,
        AllStates
    }
}
