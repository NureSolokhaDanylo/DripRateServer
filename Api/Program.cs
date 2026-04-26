using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Api.Extensions;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using SharedSettings;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

services.AddInfrastructure();
services.AddApplication();
services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var validationErrors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                {
                    Code = "General.Validation",
                    Message = e.ErrorMessage,
                    Field = kvp.Key
                }))
                .ToList();

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
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

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails);
        };
    });
services.AddHttpClient();
services.AddOpenApiDocumentation();

services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

var host = builder.Build();

await host.InitializeDatabaseAsync();

host.UseExceptionHandler();

if (host.Environment.IsDevelopment())
{
    host.UseOpenApiDocumentation();
}

host.UseCors();
host.UseAuthentication();
host.UseAuthorization();

host.MapControllers();

await host.RunAsync();
