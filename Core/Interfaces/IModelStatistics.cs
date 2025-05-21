using System;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for AI model statistics
    /// </summary>
    public interface IModelStatistics
    {
        /// <summary>
        /// Current accuracy metric of the model (0.0-1.0)
        /// </summary>
        double Accuracy { get; }
        
        /// <summary>
        /// Total number of samples/feedback entries used for training
        /// </summary>
        int TotalSamples { get; }
        
        /// <summary>
        /// Total number of feedback entries used for training (alias for TotalSamples for backward compatibility)
        /// </summary>
        int TotalFeedbackCount { get => TotalSamples; }
        
        /// <summary>
        /// Date and time when the model was last updated/trained
        /// </summary>
        DateTime LastUpdated { get; }
        
        /// <summary>
        /// Date and time when the model was last trained (alias for LastUpdated for backward compatibility)
        /// </summary>
        DateTime? LastTrainingTime { get => LastUpdated; }
        
        /// <summary>
        /// Current version of the model
        /// </summary>
        string ModelVersion { get; }
        
        /// <summary>
        /// Number of training iterations performed
        /// </summary>
        int TrainingIterations { get; }
        
        /// <summary>
        /// Whether the model is currently active and being used
        /// </summary>
        bool IsActive { get; }
    }
}