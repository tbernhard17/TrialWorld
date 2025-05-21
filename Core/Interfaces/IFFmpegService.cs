using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.StreamInfo;
using TrialWorld.Core.Models.Analysis;

// Use an alias to resolve ambiguity
//using CoreMediaConversionOptions = TrialWorld.Core.Models.MediaConversionOptions;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Provides a simplified abstraction over FFmpeg, primarily for generic command execution 
    /// and potentially managing long-running process status.
    /// Specific operations like conversion, trimming, etc., should use dedicated services.
    /// </summary>
    public interface IFFmpegService
    {
        /// <summary>
        /// Gets the installed FFmpeg version information.
        /// </summary>
        Task<string> GetFFmpegVersionAsync();
        
        /// <summary>
        /// Attempts to cancel any ongoing operation initiated by this service instance.
        /// </summary>
        Task CancelAsync();
    }
}