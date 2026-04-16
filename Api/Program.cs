using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Api.Extensions;
using Application.Handlers;
using Microsoft.EntityFrameworkCore;
using SharedSettings;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddInfrastructure();
services.AddApplication();
services.AddControllers();
services.AddHttpClient();
services.AddSwaggerDocumentation();

var host = builder.Build();

await host.InitializeDatabaseAsync();

if (host.Environment.IsDevelopment())
{
    host.UseSwaggerDocumentation();
}

host.UseAuthentication();
host.UseAuthorization();

await host.RunAsync();
