using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Production-ready FFmpegMediaPlayer using FFplay for async playback control.
    /// </summary>
    public class FFmpegMediaPlayer : IMediaPlaybackService
    {
        private readonly string _ffplayPath;
        private readonly ILogger<FFmpegMediaPlayer> _logger;
        private Process? _playbackProcess;

        public FFmpegMediaPlayer(string ffplayPath, ILogger<FFmpegMediaPlayer> logger)
        {
            _ffplayPath = ffplayPath ?? "ffplay";
            _logger = logger;
        }

        public async Task PlayAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Media file not found.", filePath);
            await StopAsync();
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffplayPath,
                Arguments = $"-autoexit -nodisp \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _playbackProcess = Process.Start(startInfo);
            _logger.LogInformation("Started playback: {FilePath}", filePath);
        }

        public Task PauseAsync(CancellationToken cancellationToken = default)
        {
            // FFplay does not support programmatic pause via CLI; would require IPC or custom build.
            _logger.LogWarning("Pause is not supported in FFmpegMediaPlayer.");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_playbackProcess != null && !_playbackProcess.HasExited)
            {
                _playbackProcess.Kill();
                await _playbackProcess.WaitForExitAsync(cancellationToken);
                _logger.LogInformation("Stopped playback.");
            }
            _playbackProcess?.Dispose();
            _playbackProcess = null;
        }

        public void Dispose()
        {
            if (_playbackProcess != null && !_playbackProcess.HasExited)
                _playbackProcess.Kill();
            _playbackProcess?.Dispose();
        }
    }
}