using Application.Interfaces;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middlewares;

public sealed class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            if (await currentUser.IsBannedAsync(context.RequestAborted))
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Extensions = { { "code", AuthErrors.UserBannedCode } },
                    Detail = "Your account has been banned."
                };

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(problemDetails);
                return;
            }
        }

        await _next(context);
    }
}
