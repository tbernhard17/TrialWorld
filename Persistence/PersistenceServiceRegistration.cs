using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Interfaces.Persistence;
using TrialWorld.Core.Models;
using TrialWorld.Persistence.Data;
using TrialWorld.Persistence.Repositories;
namespace TrialWorld.Persistence
{
    /// <summary>
    /// Contains extension methods for registering persistence services with the dependency injection container.
    /// Adheres to Rule #2 (Register 'em clean in DI).
    /// </summary>
    public static class PersistenceServiceRegistration
    {
        /// <summary>
        /// Adds persistence layer services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Enable retry logic for transient failures (Rule #8)
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // Register repositories
            // Using Scoped lifetime for repositories tied to the DbContext's scope
            services.AddScoped<IMediaMetadataRepository, MediaMetadataRepository>();
            services.AddScoped<ITranscriptRepository, TranscriptRepository>();
            services.AddScoped<ISearchFeedbackRepository, SearchFeedbackRepository>();

            // Register generic repository for different analysis types
            services.AddScoped(typeof(IAnalysisResultRepository<>), typeof(AnalysisResultRepository<>));

            return services;
        }
    }
}