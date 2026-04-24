using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

public sealed class OperationIdTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            operation.OperationId ??= controllerActionDescriptor.ActionName;
        }

        return Task.CompletedTask;
    }
}
