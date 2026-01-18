using Employee.Services.Services;
using Employee.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Employee.Services.Extensions
{
    public static class EmployeeServiceCollectionExtensions
    {
        public static IServiceCollection AddEmployeeServices(this IServiceCollection services)
        {
            services.AddScoped<IWorkCalculationService, WorkCalculationService>();
            services.AddScoped<ICsvParser, CsvParser>();
            services.AddScoped<IDateParser, DateParser>();

            return services;
        }
    }
}
