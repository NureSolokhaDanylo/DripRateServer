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
services.AddControllers();
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
