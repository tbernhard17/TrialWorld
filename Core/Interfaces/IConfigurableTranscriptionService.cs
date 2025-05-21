using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for transcription services that allow runtime configuration changes
    /// </summary>
    public interface IConfigurableTranscriptionService
    {
        /// <summary>
        /// Sets the AI model to use for transcription
        /// </summary>
        /// <param name="model">The model to use</param>
        void SetModel(AssemblyAIModel model);
        
        /// <summary>
        /// Gets the currently selected AI model
        /// </summary>
        /// <returns>The current model</returns>
        AssemblyAIModel GetCurrentModel();
    }
}
