using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

public sealed class SchemaTypeTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
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
                schema.Type = type & ~JsonSchemaType.String;
            }
        }

        // Fix missing Null type for nullable properties/parameters
        var targetType = context.JsonPropertyInfo?.PropertyType ?? context.ParameterDescription?.Type;
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
