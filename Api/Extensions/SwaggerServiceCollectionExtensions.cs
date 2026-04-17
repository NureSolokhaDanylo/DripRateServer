using Api.OpenApi;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace Api.Extensions;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title = "DripRate Server API";
                document.Info.Version = "v1";
                document.Info.Description = "API for DripRate Server application";

                return Task.CompletedTask;
            });

            options.AddOperationTransformer<ErrorCodesTransformer>();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            
            // Add YAML endpoint
            app.MapGet("/openapi/swagger.yaml", async (HttpClient httpClient) =>
            {
                try
                {
                    var scheme = app.Urls.FirstOrDefault()?.StartsWith("https") ?? false ? "https" : "http";
                    var host = app.Urls.FirstOrDefault()?.Replace("https://", "").Replace("http://", "") ?? "localhost";
                    var jsonUrl = $"{scheme}://{host}/openapi/swagger.json";
                    
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
            .WithName("OpenAPI YAML");
        }
        
        return app;
    }
}
