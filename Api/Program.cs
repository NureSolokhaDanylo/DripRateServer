using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Environment.ContentRootPath);
var host = builder.Build();

host.Run();
