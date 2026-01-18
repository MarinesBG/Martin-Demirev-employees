using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Employee.API.Filters
{
    public class FormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Параметри, които са директно IFormFile / collection
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) ||
                            p.ParameterType == typeof(IFormFileCollection) ||
                            p.ParameterType == typeof(IEnumerable<IFormFile>))
                .Select(p => p.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            // Проверка за IFormFile свойства в DTO параметри
            var dtoFileProps = context.MethodInfo.GetParameters()
                .SelectMany(p =>
                {
                    var paramType = p.ParameterType;
                    // само за комплексни типове (изключваме примитиви и string)
                    if (paramType.IsPrimitive || paramType == typeof(string)) return Enumerable.Empty<string>();
                    return paramType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(prop => prop.PropertyType == typeof(IFormFile) ||
                                       prop.PropertyType == typeof(IFormFileCollection) ||
                                       prop.PropertyType == typeof(IEnumerable<IFormFile>))
                        .Select(prop => prop.Name);
                })
                .ToList();

            var allFileNames = fileParams.Concat(dtoFileProps).Distinct().ToList();

            if (!allFileNames.Any())
                return;

            operation.RequestBody ??= new OpenApiRequestBody { Content = new Dictionary<string, OpenApiMediaType>() };

            var schemaProps = allFileNames.ToDictionary(
                name => name,
                name => (OpenApiSchema)new OpenApiSchema { Type = "string", Format = "binary", Description = "Upload CSV file" }
            );

            operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = schemaProps,
                    Required = schemaProps.Keys.ToHashSet()
                },
                Encoding = schemaProps.Keys.ToDictionary(
                    name => name,
                    name => new OpenApiEncoding { ContentType = "text/csv", Style = ParameterStyle.Form }
                )
            };

            // Премахваме параметрите, които вече са в requestBody (само директни параметри)
            foreach (var pName in fileParams)
            {
                var toRemove = operation.Parameters.FirstOrDefault(op => op.Name == pName);
                if (toRemove != null) operation.Parameters.Remove(toRemove);
            }
        }
    }
}
