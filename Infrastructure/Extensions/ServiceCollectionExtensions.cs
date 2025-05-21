using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TrialWorld.AssemblyAIDiagnostic.Configuration;
using TrialWorld.AssemblyAIDiagnostic.Repositories;
using TrialWorld.AssemblyAIDiagnostic.Services;

namespace TrialWorld.AssemblyAIDiagnostic.Extensions
{
    /// <summary>
    /// Extension methods for registering AssemblyAI services with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds AssemblyAI direct API services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddAssemblyAIDirectApi(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options with validation
            services.AddOptions<AssemblyAIOptions>()
                .Bind(configuration.GetSection("AssemblyAI"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Configure Serilog
            ConfigureSerilog(configuration);

            // Register HttpClient with resilience policies
            services.AddHttpClient<IAssemblyAIDirectApiService, AssemblyAIDirectApiService>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<AssemblyAIOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                    client.DefaultRequestHeaders.Add("User-Agent", "TrialWorld.AssemblyAIDiagnostic/1.0");
                })
                .AddPolicyHandler((serviceProvider, _) => GetRetryPolicy(serviceProvider))
                .AddPolicyHandler((serviceProvider, _) => GetCircuitBreakerPolicy(serviceProvider))
                .AddPolicyHandler((serviceProvider, _) => GetTimeoutPolicy(serviceProvider))
                .AddHttpMessageHandler(() => new LoggingDelegatingHandler());

            // Register audio extraction service
            services.AddSingleton<IAudioExtractionService, AudioExtractionService>();
            
            // Register transcription repository
            services.AddSingleton<ITranscriptionRepository, TranscriptionRepository>();
            
            // Register diagnostics services
            if (configuration.GetSection("Diagnostics").GetValue<bool>("EnablePerformanceMetrics", false))
            {
                services.AddSingleton<DiagnosticsService>();
            }

            return services;
        }

        /// <summary>
        /// Configures Serilog from configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        private static void ConfigureSerilog(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        /// <summary>
        /// Creates a retry policy for HTTP requests.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The retry policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<AssemblyAIOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<IAssemblyAIDirectApiService>>();

            return HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5xx and 408 status codes
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 429 status code
                .WaitAndRetryAsync(
                    retryCount: options.MaxRetryAttempts,
                    sleepDurationProvider: (retryAttempt, response, context) =>
                    {
                        // Calculate exponential backoff with jitter
                        var backoffDelay = Math.Min(
                            options.MaxRetryDelayMs,
                            options.InitialRetryDelayMs * Math.Pow(options.RetryBackoffExponent, retryAttempt - 1));
                        
                        // Add jitter to prevent thundering herd
                        var jitteredDelay = backoffDelay * (0.8 + (new Random().NextDouble() * 0.4));
                        
                        // Check for Retry-After header
                        if (response.Result?.Headers?.RetryAfter != null && 
                            response.Result.Headers.RetryAfter.Delta.HasValue)
                        {
                            var retryAfterDelay = response.Result.Headers.RetryAfter.Delta.Value.TotalMilliseconds;
                            jitteredDelay = Math.Max(jitteredDelay, retryAfterDelay);
                        }
                        
                        return TimeSpan.FromMilliseconds(jitteredDelay);
                    },
                    onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning(
                            "Retry {RetryAttempt} after {RetryDelay}ms due to {StatusCode}: {ReasonPhrase}",
                            retryAttempt,
                            timespan.TotalMilliseconds,
                            outcome.Result?.StatusCode,
                            outcome.Result?.ReasonPhrase);

                        if (options.EnableDetailedLogging && outcome.Exception != null)
                        {
                            logger.LogWarning(outcome.Exception, "Exception on retry attempt {RetryAttempt}", retryAttempt);
                        }

                        // Add diagnostic information
                        context["RetryCount"] = retryAttempt;
                        context["RetryDelay"] = timespan.TotalMilliseconds;

                        await Task.CompletedTask;
                    });
        }

        /// <summary>
        /// Creates a circuit breaker policy for HTTP requests.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The circuit breaker policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<AssemblyAIOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<IAssemblyAIDirectApiService>>();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: options.CircuitBreakerFailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                    onBreak: (outcome, breakDuration) =>
                    {
                        logger.LogError(
                            "Circuit breaker opened for {BreakDuration}s due to {FailureCount} failures",
                            breakDuration.TotalSeconds,
                            options.CircuitBreakerFailureThreshold);
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation("Circuit breaker half-open");
                    });
        }

        /// <summary>
        /// Creates a timeout policy for HTTP requests.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The timeout policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<AssemblyAIOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<IAssemblyAIDirectApiService>>();

            return Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(options.RequestTimeoutSeconds),
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: async (context, timespan, _, _) =>
                {
                    logger.LogWarning("Request timed out after {Timeout}s", timespan.TotalSeconds);
                    await Task.CompletedTask;
                });
        }
    }

    /// <summary>
    /// Logging delegating handler for HTTP requests.
    /// </summary>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        /// <summary>
        /// Sends the request and logs the details.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Create a correlation ID for the request if not present
            if (!request.Headers.Contains("X-Correlation-ID"))
            {
                var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
                request.Headers.Add("X-Correlation-ID", correlationId);
            }

            // Log the request
            Log.Information("HTTP {Method} {Uri} started", request.Method, request.RequestUri);

            // Start a timer
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Send the request
                var response = await base.SendAsync(request, cancellationToken);

                // Log the response
                stopwatch.Stop();
                Log.Information(
                    "HTTP {Method} {Uri} completed with {StatusCode} in {ElapsedMs}ms",
                    request.Method,
                    request.RequestUri,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception
                stopwatch.Stop();
                Log.Error(
                    ex,
                    "HTTP {Method} {Uri} failed after {ElapsedMs}ms",
                    request.Method,
                    request.RequestUri,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }

    /// <summary>
    /// Service for collecting diagnostic information.
    /// </summary>
    public class DiagnosticsService
    {
        private readonly ILogger<DiagnosticsService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DiagnosticsService(ILogger<DiagnosticsService> logger)
        {
            _logger = logger;
            _logger.LogInformation("Diagnostics service initialized");
        }

        /// <summary>
        /// Records a diagnostic event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="durationMs">The duration in milliseconds.</param>
        /// <param name="metadata">Additional metadata.</param>
        public void RecordEvent(string eventName, long durationMs, object? metadata = null)
        {
            _logger.LogInformation(
                "Diagnostic event: {EventName} completed in {DurationMs}ms",
                eventName,
                durationMs);

            if (metadata != null)
            {
                _logger.LogDebug("Event metadata: {@Metadata}", metadata);
            }
        }
    }
}
