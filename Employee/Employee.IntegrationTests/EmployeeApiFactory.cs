using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Employee.IntegrationTests
{
    public class EmployeeApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // You can override services here if needed
                // For example, replace database with in-memory, mock external services, etc.
            });

            builder.UseEnvironment("Testing");
        }
    }
}