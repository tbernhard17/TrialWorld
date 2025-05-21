using System;
using System.Threading.Tasks;

namespace TrialWorld.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for the mapper service that handles conversions between DTOs and domain models
    /// </summary>
    public interface IMapperService
    {
        /// <summary>
        /// Maps from a source object to a new destination object
        /// </summary>
        TDestination Map<TSource, TDestination>(TSource source);
        
        /// <summary>
        /// Maps from a source object to an existing destination object
        /// </summary>
        void Map<TSource, TDestination>(TSource source, TDestination destination);
        
        /// <summary>
        /// Asynchronously maps a collection using a provided mapping function
        /// </summary>
        Task<TDestination> MapAsync<TSource, TDestination>(
            TSource source, 
            Func<TSource, Task<TDestination>> mappingFunc);
    }
}
