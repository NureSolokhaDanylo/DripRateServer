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
            var array = new JsonArray();
            foreach (var code in allCodes)
            {
                array.Add(code);
            }

            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions["x-error-codes"] = new JsonNodeExtension(array);
        }

        return Task.CompletedTask;
    }
}
