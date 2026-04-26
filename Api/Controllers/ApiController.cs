using ErrorOr;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    protected IActionResult Problem(Error error)
    {
        if (error.Type == ErrorType.Validation)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: error.Description,
                extensions: new Dictionary<string, object?> { { "code", "General.Validation" } });
        }

        if (ApiErrorRegistry.TryGet(error.Code, out var metadata))
        {
            return Problem(
                statusCode: metadata.StatusCode,
                title: error.Description,
                extensions: new Dictionary<string, object?> { { "code", error.Code } });
        }

        var fallbackStatusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(
            statusCode: fallbackStatusCode,
            title: "An internal server error occurred.",
            extensions: new Dictionary<string, object?> { { "code", "General.InternalServerError" } });
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var validationErrors = errors.Select(e => new
        {
            Code = e.Code,
            Message = e.Description,
            Field = e.Metadata?.TryGetValue("FieldName", out var fieldName) == true ? fieldName?.ToString() : null
        }).ToList();

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Extensions = 
            { 
                { "code", "General.Validation" },
                { "validationErrors", validationErrors } 
            }
        };

        return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }
}
