using System;

namespace TrialWorld.Infrastructure.Models.FFmpeg
{
    /// <summary>
    /// Represents the result of executing a raw FFmpeg command.
    /// </summary>
    public class FFmpegResult
    {
        /// <summary>
        /// Gets or sets the exit code of the FFmpeg process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the standard output captured from the FFmpeg process.
        /// </summary>
        public string Output { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the standard error captured from the FFmpeg process.
        /// </summary>
        public string Error { get; set; } = string.Empty;
    }
}