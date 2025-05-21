using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Exceptions; // Assuming FFmpegProcessingException might be generalized or moved
using TrialWorld.Infrastructure.Models.FFmpeg; // Added for FFmpegOptions

namespace TrialWorld.Infrastructure.Utilities
{
    /// <summary>
    /// Provides functionality to run external processes.
    /// </summary>
    public class ProcessRunner : IProcessRunner
    {
        private readonly ILogger<ProcessRunner> _logger;
        private readonly FFmpegOptions _options;

        public ProcessRunner(ILogger<ProcessRunner> logger, IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async Task<string> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = _options.WorkingDirectory
            };

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var processCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) { outputBuilder.AppendLine(e.Data); } };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) { errorBuilder.AppendLine(e.Data); } };
            process.Exited += (sender, e) => processCompletionSource.TrySetResult(true);

            using var externalCancellationRegistration = cancellationToken.Register(() =>
            {
                if (!processCompletionSource.Task.IsCompleted)
                {
                    try
                    {
                        if (!process.HasExited) process.Kill(true);
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                    {
                         _logger.LogWarning(ex, "Failed to forcefully kill process {ProcessId} during cancellation.", process.Id);
                    }
                    processCompletionSource.TrySetCanceled(cancellationToken);
                }
            });

            using var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(_options.DefaultTimeout);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            if (_options.EnableLogging)
            {
                _logger.LogDebug("Executing process: {FileName} {Arguments} in {WorkingDirectory} with timeout {Timeout}", 
                    fileName, arguments, startInfo.WorkingDirectory, _options.DefaultTimeout);
            }

            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException($"Failed to start process: {fileName}");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await processCompletionSource.Task.WaitAsync(linkedCts.Token);

                if (process.ExitCode != 0)
                {
                    var error = errorBuilder.ToString().Trim();
                    if (_options.EnableLogging) 
                    {
                        _logger.LogError("Process {FileName} exited with code {ExitCode}. Error: {Error}", fileName, process.ExitCode, error);
                    }
                    throw new ExternalProcessException($"Process {fileName} exited with code {process.ExitCode}: {error}", process.ExitCode, error);
                }

                var output = outputBuilder.ToString().Trim();
                if (_options.EnableLogging) 
                {
                    _logger.LogDebug("Process {FileName} completed successfully. Output length: {OutputLength}", fileName, output.Length);
                }
                return output;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Process {FileName} execution was cancelled externally.", fileName);
                throw;
            }
            catch (Exception ex) when (ex is not ExternalProcessException && ex is not TimeoutException)
            {
                _logger.LogError(ex, "An unexpected error occurred while running process {FileName}.", fileName);
                throw new ExternalProcessException($"An unexpected error occurred while running process {fileName}. See inner exception for details.", ex);
            }
            finally
            {
            }
        }

        /// <inheritdoc />
        public async Task RunProcessWithProgressAsync(string fileName, string arguments, IProgress<string> progressCallback, CancellationToken cancellationToken)
        {
             if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = _options.WorkingDirectory
            };

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var processCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => 
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    progressCallback.Report(e.Data);
                }
            };
            process.Exited += (sender, e) => processCompletionSource.TrySetResult(true);

            using var externalCancellationRegistration = cancellationToken.Register(() =>
            {
                if (!processCompletionSource.Task.IsCompleted)
                {
                    try
                    {
                        if (!process.HasExited) process.Kill(true);
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                    {
                         _logger.LogWarning(ex, "Failed to forcefully kill process {ProcessId} during cancellation.", process.Id);
                    }
                    processCompletionSource.TrySetCanceled(cancellationToken);
                }
            });
            
            using var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(_options.DefaultTimeout);
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            if (_options.EnableLogging)
            {
                 _logger.LogDebug("Executing process with progress: {FileName} {Arguments} in {WorkingDirectory} with timeout {Timeout}", 
                    fileName, arguments, startInfo.WorkingDirectory, _options.DefaultTimeout);
            }

            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException($"Failed to start process: {fileName}");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await processCompletionSource.Task.WaitAsync(linkedCts.Token);

                if (process.ExitCode != 0)
                {
                    var error = errorBuilder.ToString().Trim();
                    var stdOut = outputBuilder.ToString().Trim();
                    if (_options.EnableLogging) 
                    {
                        _logger.LogError("Process {FileName} exited with code {ExitCode}. Error: {Error}. StdOut: {StdOut}", 
                            fileName, process.ExitCode, error, stdOut);
                    }
                    throw new ExternalProcessException($"Process {fileName} exited with code {process.ExitCode}: {error}", process.ExitCode, error);
                }
                
                if (_options.EnableLogging)
                {
                    _logger.LogDebug("Process {FileName} completed successfully.", fileName);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Process {FileName} execution was cancelled externally.", fileName);
                throw;
            }
            catch (Exception ex) when (ex is not ExternalProcessException && ex is not TimeoutException)
            {
                _logger.LogError(ex, "An unexpected error occurred while running process {FileName}.", fileName);
                throw new ExternalProcessException($"An unexpected error occurred while running process {fileName}. See inner exception for details.", ex);
            }
            finally
            {
            }
        }
    }
} 