using Api.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace Api.Extensions;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = "DripRate Server API";
                document.Info.Version = "v1";
                document.Info.Description = "API for DripRate Server application";

                // Remove OpenAPI spec endpoints from the document itself
                var openApiPaths = document.Paths
                    .Where(p => p.Key.Contains("/openapi/"))
                    .Select(p => p.Key)
                    .ToList();

                foreach (var path in openApiPaths)
                {
                    document.Paths.Remove(path);
                }

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                if (document.Components?.Schemas?.TryGetValue("ProblemDetails", out var schema) == true)
                {
                    schema.Properties.Add("code", new OpenApiSchema { Type = JsonSchemaType.String });
                    schema.Properties.Add("validationErrors", new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                ["code"] = new OpenApiSchema { Type = JsonSchemaType.String },
                                ["message"] = new OpenApiSchema { Type = JsonSchemaType.String },
                                ["field"] = new OpenApiSchema { Type = JsonSchemaType.String }
                            }
                        }
                    });
                }

                return Task.CompletedTask;
            });

            options.AddOperationTransformer<OperationIdTransformer>();
            options.AddOperationTransformer<SecurityAndErrorCodesTransformer>();
            options.AddOperationTransformer<MultipartFormDataTransformer>();
            options.AddSchemaTransformer<SchemaTypeTransformer>();
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi("/openapi/openapi.json");
            
            // Add YAML endpoint
            app.MapGet("/openapi/openapi.yaml", async (HttpContext context, HttpClient httpClient) =>
            {
                try
                {
                    var scheme = context.Request.Scheme;
                    var host = context.Request.Host;
                    var jsonUrl = $"{scheme}://{host}/openapi/openapi.json";
                    
                    var jsonContent = await httpClient.GetStringAsync(jsonUrl);
                    var jsonObject = JsonSerializer.Deserialize<object>(jsonContent);
                    
                    var serializer = new SerializerBuilder()
                        .JsonCompatible()
                        .Build();
                    
                    var yamlContent = serializer.Serialize(jsonObject);
                    
                    return Results.Content(yamlContent, "application/yaml");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error generating YAML: {ex.Message}", statusCode: 500);
                }
            })
            .WithName("OpenAPI YAML")
            .ExcludeFromDescription();
        }
        
        return app;
    }
}
