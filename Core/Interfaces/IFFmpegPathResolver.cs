using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for resolving the path to FFmpeg executable.
    /// </summary>
    public interface IFFmpegPathResolver
    {
        /// <summary>
        /// Gets the path to the FFmpeg executable.
        /// </summary>
        /// <returns>The path to the FFmpeg executable.</returns>
        string GetFFmpegExecutablePath();
        
        /// <summary>
        /// Gets the path to the FFprobe executable.
        /// </summary>
        /// <returns>The path to the FFprobe executable.</returns>
        string GetFFprobePath();
        
        /// <summary>
        /// Asynchronously validates that FFmpeg is installed and accessible.
        /// </summary>
        /// <returns>True if FFmpeg is valid and accessible, otherwise false.</returns>
        Task<bool> ValidateFFmpegAsync();
    }
}
