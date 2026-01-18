using Microsoft.Extensions.DependencyInjection;

namespace Employee.Mapping.Extensions
{
    public static class AutoMapperServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
