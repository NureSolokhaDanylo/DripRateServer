using Api.Attributes;
using Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Api.OpenApi;

public sealed class SecurityAndErrorCodesTransformer : IOpenApiOperationTransformer
{
    private const string ValidationCode = "General.Validation";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return Task.CompletedTask;
        }

        context.Document.Components ??= new OpenApiComponents();
        context.Document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        var statusCodesByBusinessCode = new Dictionary<int, HashSet<string>>();
        
        // 1. Map business errors from [ApiErrors]
        var attributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ApiErrorsAttribute), true)
            .Cast<ApiErrorsAttribute>()
            .ToArray();

        foreach (var code in attributes.SelectMany(a => a.Codes).Distinct(StringComparer.Ordinal))
        {
            if (ApiErrorRegistry.TryGet(code, out var metadata))
            {
                AddCode(statusCodesByBusinessCode, metadata.StatusCode, code);
            }
        }

        // 2. Add validation error code if action has parameters
        if (controllerActionDescriptor.Parameters.Any())
        {
            AddCode(statusCodesByBusinessCode, 400, ValidationCode);
        }

        // 3. Handle Security and Authorization errors
        var endpointMetadata = controllerActionDescriptor.EndpointMetadata;
        var hasAuthorize = endpointMetadata.OfType<AuthorizeAttribute>().Any();
        var hasAllowAnonymous = endpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        var authorizeWithError = endpointMetadata.OfType<AuthorizeWithErrorAttribute>().FirstOrDefault();

        if ((hasAuthorize || authorizeWithError is not null) && !hasAllowAnonymous)
        {
            // Add Security Requirement (the "lock" in UI)
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            var scheme = new OpenApiSecuritySchemeReference("Bearer", context.Document);
            operation.Security.Add(new OpenApiSecurityRequirement { [scheme] = new List<string>() });

            // Add specific error code for Unauthorized
            var errorCode = authorizeWithError?.ErrorCode ?? AuthErrors.UnauthorizedCode;
            if (ApiErrorRegistry.TryGet(errorCode, out var authMetadata))
            {
                AddCode(statusCodesByBusinessCode, authMetadata.StatusCode, errorCode);
            }
            else
            {
                AddCode(statusCodesByBusinessCode, 401, AuthErrors.UnauthorizedCode);
            }
        }

        // 4. Apply schemas to responses
        foreach (var kv in statusCodesByBusinessCode)
        {
            var statusCode = kv.Key;
            var responseKey = statusCode.ToString();
            var enumName = $"{operation.OperationId}_{responseKey}ErrorCode";

            EnsureCodeEnumSchema(context, enumName, kv.Value);
            EnsureProblemResponse(operation, context, responseKey, statusCode);
            ExtendCodeSchema(operation.Responses[responseKey], enumName, statusCode, context);
        }

        return Task.CompletedTask;
    }

    private static void AddCode(Dictionary<int, HashSet<string>> statusToCodes, int statusCode, string code)
    {
        if (!statusToCodes.TryGetValue(statusCode, out var codes))
        {
            codes = new HashSet<string>(StringComparer.Ordinal);
            statusToCodes[statusCode] = codes;
        }

        codes.Add(code);
    }

    private static void EnsureCodeEnumSchema(
        OpenApiOperationTransformerContext context,
        string enumName,
        IEnumerable<string> codes)
    {
        if (context.Document.Components?.Schemas?.ContainsKey(enumName) == true)
        {
            return;
        }

        var enumValues = codes
            .OrderBy(c => c, StringComparer.Ordinal)
            .Select(c => JsonValue.Create(c) as JsonNode)
            .ToList();

        context.Document.Components!.Schemas!.Add(enumName, new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Enum = enumValues,
        });
    }

    private static void EnsureProblemResponse(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        string responseKey,
        int statusCode)
    {
        if (!operation.Responses.TryGetValue(responseKey, out var response))
        {
            response = new OpenApiResponse { Description = GetResponseDescription(statusCode) };
            operation.Responses[responseKey] = response;
        }

        if (response.Content is null)
        {
            if (response is OpenApiResponse mutableResponse)
            {
                mutableResponse.Content = new Dictionary<string, OpenApiMediaType>();
            }
            else
            {
                response = new OpenApiResponse
                {
                    Description = response.Description,
                    Content = new Dictionary<string, OpenApiMediaType>()
                };
                operation.Responses[responseKey] = response;
            }
        }

        if (!response.Content.ContainsKey("application/json"))
        {
            response.Content.Add("application/json", new OpenApiMediaType
            {
                Schema = new OpenApiSchemaReference("ProblemDetails", context.Document)
            });
        }

        if (!response.Content.ContainsKey("text/json"))
        {
            response.Content.Add("text/json", new OpenApiMediaType
            {
                Schema = new OpenApiSchemaReference("ProblemDetails", context.Document)
            });
        }
    }

    private static void ExtendCodeSchema(IOpenApiResponse response, string enumName, int statusCode, OpenApiOperationTransformerContext context)
    {
        if (response.Content is null)
        {
            return;
        }

        foreach (var content in response.Content.Values)
        {
            var baseSchema = content.Schema ?? new OpenApiSchemaReference("ProblemDetails", context.Document);
            var schema = new OpenApiSchema();
            schema.AllOf ??= new List<IOpenApiSchema>();
            schema.AllOf.Add(baseSchema);

            var properties = new Dictionary<string, IOpenApiSchema>
            {
                ["code"] = new OpenApiSchemaReference(enumName, context.Document),
            };

            if (statusCode == 400)
            {
                properties["validationErrors"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array | JsonSchemaType.Null,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["code"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["message"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["field"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null }
                        }
                    }
                };
            }

            var extensionSchema = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = properties
            };

            schema.AllOf.Add(extensionSchema);
            content.Schema = schema;
        }
    }

    private static string GetResponseDescription(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        500 => "Internal Server Error",
        _ => "Error",
    };
}
