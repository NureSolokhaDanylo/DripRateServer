using Infrastructure.Persistence;

namespace Api.Extensions;

public static class HostExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

        await initializer.InitializeAsync();
    }
}
