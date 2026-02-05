using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RC.API
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // multipart/form-data olan endpoint'leri kontrol et
            var consumesAttribute = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
                .FirstOrDefault();

            if (consumesAttribute == null ||
                !consumesAttribute.ContentTypes.Contains("multipart/form-data"))
                return;

            // IFormFile parametrelerini bul
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) ||
                           (p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericTypeDefinition() == typeof(List<>) &&
                            p.ParameterType.GetGenericArguments()[0] == typeof(IFormFile)))
                .ToList();

            if (!fileParams.Any())
                return;

            // Mevcut form parametrelerini kaldır
            var parametersToRemove = operation.Parameters
                .Where(p => fileParams.Any(fp => fp.Name == p.Name))
                .ToList();

            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }

            // RequestBody schema oluştur
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var fileParam in fileParams)
            {
                if (fileParam.ParameterType == typeof(IFormFile))
                {
                    // Tek dosya
                    schema.Properties[fileParam.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else
                {
                    // Çoklu dosya (List<IFormFile>)
                    schema.Properties[fileParam.Name!] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    };
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = schema
                    }
                }
            };
        }
    }
}