using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Epic5Task.Application.Filters;

public class AddDescriptionsToSchemaAttributes : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        var isNullableEnum = false;

        // Розкриваємо Nullable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying is null || !underlying.IsEnum)
                return;

            type = underlying;
            isNullableEnum = true;
        }
        else if (!type.IsEnum)
        {
            return;
        }

        // Тепер type — це справжній enum
        schema.Type = "string";
        schema.Format = null;
        schema.Nullable = schema.Nullable || isNullableEnum;

        schema.Enum = new List<IOpenApiAny>();

        foreach (var value in Enum.GetValues(type))
        {
            var name = value.ToString()!;
            var memberInfo = type.GetMember(name).FirstOrDefault();

            var enumMember = memberInfo?.GetCustomAttribute<EnumMemberAttribute>();
            if (!string.IsNullOrWhiteSpace(enumMember?.Value))
            {
                schema.Enum.Add(new OpenApiString(enumMember!.Value!));
                continue;
            }

            schema.Enum.Add(new OpenApiString(name));
        }
    }
}