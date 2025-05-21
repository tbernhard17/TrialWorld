using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TrialWorld.Application.Common.Interfaces;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Application.Services
{
    /// <summary>
    /// Service that handles mapping between DTOs and domain models
    /// This centralizes the mapping logic and provides a consistent approach
    /// </summary>
    public class MapperService : IMapperService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MapperService> _logger;

        public MapperService(IMapper mapper, ILogger<MapperService> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps from a source object to a new destination object
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            try
            {
                return _mapper.Map<TDestination>(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping from {SourceType} to {DestinationType}", 
                    typeof(TSource).Name, typeof(TDestination).Name);
                throw;
            }
        }

        /// <summary>
        /// Maps from a source object to an existing destination object
        /// </summary>
        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            try
            {
                _mapper.Map(source, destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping from {SourceType} to {DestinationType}",
                    typeof(TSource).Name, typeof(TDestination).Name);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously maps a collection using a provided mapping function
        /// </summary>
        public async Task<TDestination> MapAsync<TSource, TDestination>(
            TSource source, 
            Func<TSource, Task<TDestination>> mappingFunc)
        {
            try
            {
                return await mappingFunc(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping asynchronously from {SourceType} to {DestinationType}",
                    typeof(TSource).Name, typeof(TDestination).Name);
                throw;
            }
        }
    }
}
