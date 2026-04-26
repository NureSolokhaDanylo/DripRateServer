using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

public class MultipartFormDataTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/x-www-form-urlencoded", out var content) == true)
        {
            bool hasFile = content.Schema?.Properties?.Any(p => p.Value.Format == "binary") == true;
            
            if (hasFile)
            {
                operation.RequestBody.Content.Remove("application/x-www-form-urlencoded");
                operation.RequestBody.Content["multipart/form-data"] = content;
            }
        }
        
        return Task.CompletedTask;
    }
}
