using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Api.OpenApi;

public sealed class SchemaTypeTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var targetType = context.JsonPropertyInfo?.PropertyType ?? context.ParameterDescription?.Type;
        var underlyingType = targetType != null ? (Nullable.GetUnderlyingType(targetType) ?? targetType) : null;

        if (schema.Type.HasValue)
        {
            var type = schema.Type.Value;

            // If the schema includes a string type but also a numeric or boolean type,
            // we remove the string type to avoid union types that are poorly handled by many SDK generators.
            if (type.HasFlag(JsonSchemaType.String) &&
                (type.HasFlag(JsonSchemaType.Integer) ||
                 type.HasFlag(JsonSchemaType.Number) ||
                 type.HasFlag(JsonSchemaType.Boolean)))
            {
                // For enums, we actually WANT them to be strings (because we use JsonStringEnumConverter)
                if (underlyingType != null && underlyingType.IsEnum)
                {
                    schema.Type = JsonSchemaType.String;
                }
                else
                {
                    schema.Type = type & ~JsonSchemaType.String;
                }
            }
        }

        // If it's an enum, ensure it's represented as a string with enum values
        if (underlyingType != null && underlyingType.IsEnum)
        {
            schema.Type = (schema.Type ?? 0) | JsonSchemaType.String;
            schema.Type &= ~JsonSchemaType.Integer;
            
            schema.Enum = Enum.GetNames(underlyingType)
                .Select(name => JsonValue.Create(name) as JsonNode)
                .ToList();
        }

        // Fix missing Null type for nullable properties/parameters
        if (targetType != null && IsNullable(targetType))
        {
            schema.Type |= JsonSchemaType.Null;
        }

        return Task.CompletedTask;
    }

    private static bool IsNullable(Type type)
    {
        if (!type.IsValueType) return true; 
        if (Nullable.GetUnderlyingType(type) != null) return true;
        return false;
    }
}
