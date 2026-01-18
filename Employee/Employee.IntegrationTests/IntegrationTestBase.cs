using Xunit;

namespace Employee.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<EmployeeApiFactory>
    {
        protected readonly HttpClient Client;
        protected readonly EmployeeApiFactory Factory;

        protected IntegrationTestBase(EmployeeApiFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }

        protected MultipartFormDataContent CreateCsvFileContent(string csvContent, string fileName = "test.csv")
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csvContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            content.Add(fileContent, "file", fileName);
            return content;
        }
    }
}