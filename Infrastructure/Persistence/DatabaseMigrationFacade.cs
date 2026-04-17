using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Persistence;

public sealed class DatabaseMigrationFacade
{
    private readonly MyDbContext _context;
    private readonly ILogger<DatabaseMigrationFacade> _logger;

    public DatabaseMigrationFacade(MyDbContext context, ILogger<DatabaseMigrationFacade> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateWithRetryAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _context.Database.GetConnectionString();
        var csb = new SqlConnectionStringBuilder(connectionString);
        var databaseName = csb.InitialCatalog;
        
        // Use master to check if the server is up and database state
        csb.InitialCatalog = "master";
        var masterConnectionString = csb.ConnectionString;

        var delay = TimeSpan.FromSeconds(5);
        var attempt = 1;

        _logger.LogInformation("Database migration facade: Starting readiness check for {DatabaseName}...", databaseName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT state_desc FROM sys.databases WHERE name = @databaseName";
                command.Parameters.Add(new SqlParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128) { Value = databaseName });

                var state = (string?)await command.ExecuteScalarAsync(cancellationToken);

                if (state is null || state == "ONLINE")
                {
                    _logger.LogInformation("Database migration facade: Server is ready after {Attempt} attempts.", attempt);
                    break;
                }

                _logger.LogWarning(
                    "Database migration facade: {DatabaseName} is in state {State} (Attempt {Attempt}). Retrying in {DelaySeconds}s...",
                    databaseName, state, attempt, delay.TotalSeconds);
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(
                    "Database migration facade: Connection failed (Attempt {Attempt}): {ErrorMessage}. Retrying in {DelaySeconds}s...",
                    attempt, ex.Message, delay.TotalSeconds);
            }

            attempt++;
            await Task.Delay(delay, cancellationToken);
        }

        _logger.LogInformation("Database migration facade: Applying migrations...");
        await _context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migration facade: Migrations applied successfully.");
    }
}
