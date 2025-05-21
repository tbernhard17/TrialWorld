using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TrialWorld.Infrastructure.Transcription.Configuration;
using TrialWorld.Infrastructure.Transcription.Services;

namespace TrialWorld.Infrastructure.Transcription.Extensions
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
            // Register options
            services.Configure<AssemblyAIOptions>(configuration.GetSection("AssemblyAI"));

            // Register HttpClient with retry policy
            services.AddHttpClient<IAssemblyAIDirectApiService, AssemblyAIDirectApiService>()
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AssemblyAIOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                })
                .AddPolicyHandler(GetRetryPolicy());

            return services;
        }

        /// <summary>
        /// Creates a retry policy for HTTP requests.
        /// </summary>
        /// <returns>The retry policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5xx and 408 status codes
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 status code
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
