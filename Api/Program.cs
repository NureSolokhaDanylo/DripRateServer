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

services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

var host = builder.Build();

await host.InitializeDatabaseAsync();

host.UseExceptionHandler();

host.UseAuthentication();
host.UseAuthorization();

await host.RunAsync();
