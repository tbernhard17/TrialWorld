using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Infrastructure.Transcription.Handlers
{
    /// <summary>
    /// HTTP handler that reports progress for sending and receiving operations
    /// </summary>
    public class ProgressMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Event raised when progress is made sending a request
        /// </summary>
        public event EventHandler<HttpProgressEventArgs>? HttpSendProgress;
        
        /// <summary>
        /// Event raised when progress is made receiving a response
        /// </summary>
        public event EventHandler<HttpProgressEventArgs>? HttpReceiveProgress;
        
        /// <summary>
        /// Creates a new ProgressMessageHandler with an inner handler
        /// </summary>
        public ProgressMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }
        
        /// <summary>
        /// Sends a request with progress tracking
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add progress reporting for request body
            if (request.Content != null && HttpSendProgress != null)
            {
                // Get original content
                var originalContent = request.Content;
                
                try
                {
                    // Get content length if possible
                    var contentLength = originalContent.Headers.ContentLength;
                    
                    // Create a progress-tracking content wrapper
                    var progressContent = new ProgressContent(originalContent, bytesTransferred =>
                    {
                        // Raise the event with progress information
                        HttpSendProgress?.Invoke(this, new HttpProgressEventArgs
                        {
                            BytesTransferred = bytesTransferred,
                            TotalBytes = contentLength,
                            ProgressPercentage = contentLength.HasValue ? (int)(100 * bytesTransferred / contentLength.Value) : null
                        });
                    });
                    
                    // Replace with our tracking wrapper
                    request.Content = progressContent;
                }
                catch (Exception ex)
                {
                    // Don't let progress tracking break the actual request
                    System.Diagnostics.Debug.WriteLine($"Error setting up progress tracking: {ex.Message}");
                }
            }
            
            // Process the response with the base handler
            var response = await base.SendAsync(request, cancellationToken);
            
            // Add progress tracking for response body if needed
            if (HttpReceiveProgress != null && response.Content != null)
            {
                // Get the original response content
                var originalContent = response.Content;
                
                try
                {
                    // Get content length if available
                    var contentLength = originalContent.Headers.ContentLength;
                    
                    // Create a progress-tracking content wrapper
                    var progressContent = new ProgressContent(originalContent, bytesTransferred =>
                    {
                        // Raise the event with progress information
                        HttpReceiveProgress?.Invoke(this, new HttpProgressEventArgs
                        {
                            BytesTransferred = bytesTransferred,
                            TotalBytes = contentLength,
                            ProgressPercentage = contentLength.HasValue ? (int)(100 * bytesTransferred / contentLength.Value) : null
                        });
                    });
                    
                    // Replace with our tracking wrapper
                    response.Content = progressContent;
                }
                catch (Exception ex)
                {
                    // Don't let progress tracking break the actual response
                    System.Diagnostics.Debug.WriteLine($"Error setting up response progress tracking: {ex.Message}");
                }
            }
            
            return response;
        }
    }
    
    /// <summary>
    /// Event arguments for HTTP progress events
    /// </summary>
    public class HttpProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Number of bytes transferred
        /// </summary>
        public long BytesTransferred { get; set; }
        
        /// <summary>
        /// Total bytes to transfer, if known
        /// </summary>
        public long? TotalBytes { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100), if total is known
        /// </summary>
        public int? ProgressPercentage { get; set; }
    }
    
    /// <summary>
    /// HTTP content that tracks progress as it's read
    /// </summary>
    internal class ProgressContent : HttpContent
    {
        private readonly HttpContent _content;
        private readonly Action<long> _onProgress;
        
        /// <summary>
        /// Creates a new progress-tracking content wrapper
        /// </summary>
        public ProgressContent(HttpContent content, Action<long> onProgress)
        {
            _content = content;
            _onProgress = onProgress;
            
            // Copy the headers from the original content
            foreach (var header in content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        
        /// <summary>
        /// Gets the length of the content
        /// </summary>
        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return _content.ReadAsStreamAsync();
        }
        
        /// <summary>
        /// Computes content length
        /// </summary>
        protected override bool TryComputeLength(out long length)
        {
            if (_content.Headers.ContentLength.HasValue)
            {
                length = _content.Headers.ContentLength.Value;
                return true;
            }
            length = -1;
            return false;
        }
        
        /// <summary>
        /// Serializes content to a stream with progress reporting
        /// </summary>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var progressStream = new ProgressStream(await _content.ReadAsStreamAsync(), _onProgress);
            await progressStream.CopyToAsync(stream);
        }

        
        /// <summary>
        /// Disposes resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _content.Dispose();
            }
            
            base.Dispose(disposing);
        }
    }
    
    /// <summary>
    /// Stream wrapper that reports progress during reads
    /// </summary>
    internal class ProgressStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly Action<long> _onProgress;
        private long _bytesRead;
        
        /// <summary>
        /// Creates a new progress-tracking stream
        /// </summary>
        public ProgressStream(Stream innerStream, Action<long> onProgress)
        {
            _innerStream = innerStream;
            _onProgress = onProgress;
        }
        
        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }
        
        public override void Flush() => _innerStream.Flush();
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _innerStream.Read(buffer, offset, count);
            
            if (bytesRead > 0)
            {
                _bytesRead += bytesRead;
                _onProgress(_bytesRead);
            }
            
            return bytesRead;
        }
        
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            
            if (bytesRead > 0)
            {
                _bytesRead += bytesRead;
                _onProgress(_bytesRead);
            }
            
            return bytesRead;
        }
        
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
        
        public override void SetLength(long value) => _innerStream.SetLength(value);
        
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }
            
            base.Dispose(disposing);
        }
    }
}
