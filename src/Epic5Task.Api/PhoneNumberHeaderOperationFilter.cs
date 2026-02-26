using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Epic5Task.Api.Middlewares;

public sealed class PhoneNumberHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Не дублюємо, якщо хтось вже додав цей заголовок вручну
        if (operation.Parameters.Any(p =>
                p.In == ParameterLocation.Header &&
                string.Equals(p.Name, PhoneNumberHeaderMiddleware.HeaderName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = PhoneNumberHeaderMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Обов'язковий заголовок з номером телефону. Не може бути порожнім.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString("+380501234567")
            }
        });
    }
}