using System;
using System.ComponentModel;
namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Provides centralized state management for the application.
    /// Used to track progress and state of various operations.
    /// </summary>
    public interface ICentralStateService : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when state changes
        /// </summary>
        event EventHandler StateChanged;
        
        // Media playback state
        
        /// <summary>
        /// Gets or sets whether media is currently playing
        /// </summary>
        bool IsPlaying { get; set; }
        
        /// <summary>
        /// Gets or sets the current playback position
        /// </summary>
        TimeSpan CurrentPosition { get; set; }
        
        /// <summary>
        /// Gets or sets the total duration of the current media
        /// </summary>
        TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the playback volume (0.0-1.0)
        /// </summary>
        float Volume { get; set; }
        
        // Transcription state
        
        /// <summary>
        /// Gets whether transcription is in progress
        /// </summary>
        bool IsTranscribing { get; }
        
        /// <summary>
        /// Gets the current transcription progress (0-100)
        /// </summary>
        double TranscriptionProgress { get; }
        
        /// <summary>
        /// Gets the current transcription status message
        /// </summary>
        string TranscriptionStatus { get; }
        
        /// <summary>
        /// Gets or sets the current transcription text
        /// </summary>
        string TranscriptionText { get; set; }
        
        // Enhancement state
        
        /// <summary>
        /// Gets whether enhancement is in progress
        /// </summary>
        bool IsEnhancing { get; }
        
        /// <summary>
        /// Gets the current enhancement progress (0-100)
        /// </summary>
        double EnhancementProgress { get; }
        
        /// <summary>
        /// Gets the current enhancement status message
        /// </summary>
        string EnhancementStatus { get; }
        
        /// <summary>
        /// Gets or sets whether enhancements have been applied to the current media
        /// </summary>
        bool HasAppliedEnhancements { get; set; }
        
        // Current media info
        
        /// <summary>
        /// Gets or sets the path to the current media file
        /// </summary>
        string CurrentMediaPath { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the current media file
        /// </summary>
        string CurrentMediaName { get; set; }
        
        // Methods to update state
        
        /// <summary>
        /// Updates the media state with new file information
        /// </summary>
        /// <param name="path">Path to the media file</param>
        /// <param name="name">Name of the media file</param>
        /// <param name="duration">Duration of the media file</param>
        void UpdateMediaState(string path, string name, TimeSpan duration);
        
        /// <summary>
        /// Updates the transcription state
        /// </summary>
        /// <param name="isActive">Whether transcription is active</param>
        /// <param name="progress">Transcription progress (0-100)</param>
        /// <param name="status">Status message</param>
        void UpdateTranscriptionState(bool isActive, double progress, string status);
        
        /// <summary>
        /// Updates the enhancement state
        /// </summary>
        /// <param name="isActive">Whether enhancement is active</param>
        /// <param name="progress">Enhancement progress (0-100)</param>
        /// <param name="status">Status message</param>
        void UpdateEnhancementState(bool isActive, double progress, string status);
        
        /// <summary>
        /// Resets all state values to their defaults
        /// </summary>
        void ResetAllStates();
    }
}