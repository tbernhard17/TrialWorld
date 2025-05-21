using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TrialWorld.Application.Common.Interfaces;
using TrialWorld.Application.MappingProfiles;
using TrialWorld.Application.Services;

namespace TrialWorld.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper profiles
            services.AddAutoMapper(cfg => {
                cfg.AddProfile<DtoMappingProfile>();
                // Add any other mapping profiles here
            }, Assembly.GetExecutingAssembly());
            
            // Register the mapper service
            services.AddScoped<IMapperService, MapperService>();
            
            // Existing service registrations would be here
            
            return services;
        }
    }
}
