using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Environment.ContentRootPath);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
