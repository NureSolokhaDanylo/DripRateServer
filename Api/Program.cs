using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Api.Extensions;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using SharedSettings;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

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

host.UseAuthentication();
host.UseAuthorization();

await host.RunAsync();
