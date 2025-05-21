using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrialWorld.Infrastructure.Transcription.Handlers
{
    /// <summary>
    /// Custom HttpContent implementation that tracks upload progress of a stream.
    /// </summary>
    public class ProgressStreamContent : HttpContent
    {
        private readonly Stream _content;
        private readonly int _bufferSize;
        private readonly Action<long, long?> _progressCallback;
        private bool _contentConsumed;
        private const int DEFAULT_BUFFER_SIZE = 81920; // Increased default buffer size (80KB)
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStreamContent"/> class.
        /// </summary>
        /// <param name="content">The stream to send.</param>
        /// <param name="progress">An IProgress instance to report progress.</param>
        /// <param name="bufferSize">The buffer size for reading the stream. Default is 81920 bytes (80KB).</param>
        public ProgressStreamContent(Stream content, IProgress<long> progress, int bufferSize = DEFAULT_BUFFER_SIZE)
            : this(content, (bytesRead, totalBytes) => progress.Report(bytesRead), bufferSize)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStreamContent"/> class.
        /// </summary>
        /// <param name="content">The stream to send.</param>
        /// <param name="progressCallback">A callback to report progress.</param>
        /// <param name="bufferSize">The buffer size for reading the stream. Default is 81920 bytes (80KB).</param>
        public ProgressStreamContent(Stream content, Action<long, long?> progressCallback, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _progressCallback = progressCallback ?? throw new ArgumentNullException(nameof(progressCallback));
            _bufferSize = bufferSize;
            
            // If the stream can seek, set the content length header
            if (content.CanSeek)
            {
                Headers.ContentLength = content.Length;
            }
        }
        
        /// <summary>
        /// Releases the managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _content.Dispose();
            }
            
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Creates a stream that will be used to write out the content.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">The transport context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            if (_contentConsumed)
            {
                throw new InvalidOperationException("The content has already been consumed.");
            }
            
            _contentConsumed = true;
            
            var buffer = new byte[_bufferSize];
            long totalBytesRead = 0;
            long? totalLength = _content.CanSeek ? _content.Length : null;
            
            // Report initial progress
            _progressCallback(totalBytesRead, totalLength);
            
            int maxRetries = 3;
            int retriesCount = 0;
            
            try
            {
                int bytesRead;
                while ((bytesRead = await _content.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                {
                    try
                    {
                        await stream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                        await stream.FlushAsync().ConfigureAwait(false); // Ensure data is sent immediately
                        
                        totalBytesRead += bytesRead;
                        _progressCallback(totalBytesRead, totalLength);
                        
                        // Reset retry counter after successful chunk upload
                        retriesCount = 0;
                    }
                    catch (Exception ex) when (retriesCount < maxRetries && IsTransientError(ex))
                    {
                        // Implement retry for transient errors
                        retriesCount++;
                        int delayMs = 1000 * retriesCount; // Increasing delay
                        await Task.Delay(delayMs).ConfigureAwait(false);
                        
                        // Try the same chunk again without advancing the read position
                        _content.Position -= bytesRead;
                        continue;
                    }
                }
                
                // Final flush to ensure all data is sent
                await stream.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the error through the callback by using a negative progress value to signal error
                _progressCallback(-1, ex.Message.Length > 0 ? ex.Message.Length : null);
                throw new IOException("Error during file upload: " + ex.Message, ex);
            }
        }
        
        private bool IsTransientError(Exception ex)
        {
            // Check for network-related exceptions that might be temporary
            return ex is IOException || 
                   ex is TimeoutException || 
                   ex is HttpRequestException ||
                   (ex.InnerException != null && IsTransientError(ex.InnerException));
        }
        
        /// <summary>
        /// Computes the length of the content if possible.
        /// </summary>
        /// <param name="length">The length of the content.</param>
        /// <returns>True if length is computed; otherwise, false.</returns>
        protected override bool TryComputeLength(out long length)
        {
            if (_content.CanSeek)
            {
                length = _content.Length;
                return true;
            }
            
            length = 0;
            return false;
        }
    }
}
