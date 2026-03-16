using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Api.Extensions;
using Application.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();

var host = builder.Build();

await host.InitializeDatabaseAsync();

if (host.Environment.IsDevelopment())
{
    host.UseSwaggerDocumentation();
}

host.UseAuthentication();
host.UseAuthorization();

await host.RunAsync();
