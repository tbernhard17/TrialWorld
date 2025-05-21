using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that creates animated GIFs from video segments.
    /// </summary>
    public interface IGifCreationService
    {
        /// <summary>
        /// Creates an animated GIF from a specified segment of a video file.
        /// </summary>
        /// <param name="videoPath">Path to the input video file.</param>
        /// <param name="outputPath">Path where the generated GIF file should be saved.</param>
        /// <param name="startTime">The start time of the video segment to convert.</param>
        /// <param name="duration">The duration of the video segment to convert. If null, converts from startTime to the end.</param>
        /// <param name="frameRate">The frame rate of the output GIF.</param>
        /// <param name="width">The width of the output GIF in pixels. Height is scaled automatically.</param>
        /// <param name="quality">Quality factor (influences palette size, lower might be better for GIF). Range typically affects FFmpeg parameters.</param>
        /// <param name="dither">Whether to apply dithering for better color representation.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the generated GIF file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths are null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if frameRate, width, or quality are invalid.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input video file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if GIF creation fails for other reasons.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> CreateGifFromVideoAsync(
            string videoPath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan? duration = null,
            int frameRate = 10,
            int width = 320,
            int quality = 85, // Note: GIF quality interpretation varies; maps to palettegen max_colors
            bool dither = true,
            CancellationToken cancellationToken = default);
    }
} 