using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TrialWorld.Core.Interfaces;
using TrialWorld.Infrastructure.Transcription.Configuration;
using TrialWorld.Infrastructure.Transcription.Services;

namespace TrialWorld.Infrastructure.Transcription
{
    /// <summary>
    /// Extensions for registering transcription services with dependency injection
    /// </summary>
    public static class TranscriptionServiceRegistration
    {
        /// <summary>
        /// Add transcription services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddTranscriptionServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure AssemblyAI options from configuration
            services.Configure<AssemblyAIOptions>(configuration.GetSection("AssemblyAI"));
            
            // Create retry policy for transient HTTP errors
            var retryPolicy = GetRetryPolicy();
            
            // Register HttpClient for AssemblyAI with proper configuration and retry policy
            services.AddHttpClient<IAssemblyAIDirectApiService, AssemblyAIDirectApiService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AssemblyAIOptions>>().Value;
                
                // Configure HttpClient
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler(retryPolicy);
            
            // Register the base transcription service implementation
            services.AddTransient<TranscriptionService>();
            
            // Register the TranscriptionServiceWithSilenceDetection as the primary ITranscriptionService implementation
            // This decorates the base TranscriptionService with silence detection capabilities
            services.AddTransient<ITranscriptionService>(sp => 
            {
                var baseService = sp.GetRequiredService<TranscriptionService>();
                var silenceDetectionService = sp.GetRequiredService<ISilenceDetectionService>();
                var logger = sp.GetRequiredService<ILogger<TranscriptionServiceWithSilenceDetection>>();
                
                return new TranscriptionServiceWithSilenceDetection(baseService, silenceDetectionService, logger);
            });
            
            return services;
        }
        
        /// <summary>
        /// Creates a retry policy for handling transient HTTP errors when communicating with the AssemblyAI API.
        /// </summary>
        /// <returns>An IAsyncPolicy configured for HTTP request retries.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5XX and 408 status codes
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests) // Handle rate limiting (429)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: (retryAttempt, response, context) =>
                    {
                        // Implement exponential backoff with jitter
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                        var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
                        return baseDelay + jitter;
                    },
                    onRetryAsync: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Log retry attempts
                        if (context.TryGetValue("logger", out var loggerObj) && loggerObj is ILogger logger)
                        {
                            logger.LogWarning("Retrying AssemblyAI API request. Attempt {RetryAttempt} after {RetryDelay}ms delay.",
                                retryAttempt, timespan.TotalMilliseconds);
                        }
                        
                        return Task.CompletedTask;
                    });
        }
    }
}
