using Api.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Api.OpenApi;

public sealed class ErrorCodesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return Task.CompletedTask;
        }

        var attributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ApiErrorsAttribute), true)
            .Cast<ApiErrorsAttribute>();

        var allCodes = attributes
            .SelectMany(a => a.Codes ?? Array.Empty<string>())
            .Where(c => c != null)
            .Distinct()
            .ToArray();

        if (allCodes.Length > 0)
        {
            var enumValues = allCodes.Select(c => JsonValue.Create(c) as JsonNode).ToList();

            foreach (var response in operation.Responses)
            {
                if (response.Key.StartsWith("4") || response.Key.StartsWith("5"))
                {
                    foreach (var content in response.Value.Content)
                    {
                        var originalSchema = content.Value.Schema;
                        
                        if (originalSchema != null)
                        {
                            var schema = new OpenApiSchema();
                            schema.AllOf ??= new List<IOpenApiSchema>();
                            schema.AllOf.Add(originalSchema);
                            
                            var extensionSchema = new OpenApiSchema 
                            { 
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>()
                            };
                            
                            var codeSchema = new OpenApiSchema 
                            { 
                                Type = JsonSchemaType.String, 
                                Enum = enumValues 
                            };
                            
                            extensionSchema.Properties.Add("code", codeSchema);
                            schema.AllOf.Add(extensionSchema);
                            
                            content.Value.Schema = schema;
                        }
                    }
                }
            }

            // Extension for backward compatibility
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            var array = new JsonArray();
            foreach (var code in allCodes) array.Add(code);
            operation.Extensions["x-error-codes"] = new JsonNodeExtension(array);
        }

        return Task.CompletedTask;
    }
}
