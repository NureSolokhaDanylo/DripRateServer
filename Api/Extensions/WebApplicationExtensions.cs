using Infrastructure.Persistence;

namespace Api.Extensions;

public static class HostExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var migrationFacade = scope.ServiceProvider.GetRequiredService<DatabaseMigrationFacade>();

        await migrationFacade.MigrateWithRetryAsync();
    }
}
