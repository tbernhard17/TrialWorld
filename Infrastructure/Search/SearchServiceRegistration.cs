using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Infrastructure.Search
{
    /// <summary>
    /// Contains extension methods for registering INFRASTRUCTURE search services.
    /// </summary>
    public static class SearchServiceRegistration
    {
        /// <summary>
        /// Adds the infrastructure search services to the specified <see cref="IServiceCollection"/>.
        /// Note: ISearchService implementation (BasicSearchService) registration moved to InfrastructureServiceRegistration.
        /// Note: ISearchRanker and Application SearchService registration moved to Application layer.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddInfrastructureSearchServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register options from configuration
            services.Configure<SearchOptions>(
                configuration.GetSection("Search"));

            // Register the search indexer implementation
            // Ensure consistent lifetime (Singleton seems appropriate for Lucene index access)
            services.AddSingleton<ISearchIndexer, LuceneSearchIndexer>();

            // Register the Lucene Search Engine implementation if it's used directly
            // Assuming LuceneSearchEngine provides lower-level search functionality used by BasicSearchService
            // Or perhaps BasicSearchService encapsulates this entirely.
            // If ISearchEngine interface exists and LuceneSearchEngine implements it:
            // services.AddSingleton<ISearchEngine, LuceneSearchEngine>(); 

            return services;
        }
    }
}