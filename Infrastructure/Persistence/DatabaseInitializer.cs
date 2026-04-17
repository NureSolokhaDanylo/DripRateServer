using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly MyDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializer(
        MyDbContext context, 
        ILogger<DatabaseInitializer> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Database initialization starting...");

        await WaitForSqlServerAsync(cancellationToken);
        
        _logger.LogInformation("Applying migrations...");
        await _context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Migrations applied successfully.");

        var scripts = _serviceProvider.GetServices<IStartupScript>()
            .OrderBy(s => s.Order)
            .ToList();

        if (scripts.Any())
        {
            _logger.LogInformation("Running {Count} startup scripts...", scripts.Count);
            foreach (var script in scripts)
            {
                _logger.LogInformation("Executing script: {ScriptName}...", script.GetType().Name);
                await script.ExecuteAsync(cancellationToken);
            }
            _logger.LogInformation("All startup scripts completed.");
        }
        
        _logger.LogInformation("Database initialization completed successfully.");
    }

    private async Task WaitForSqlServerAsync(CancellationToken cancellationToken)
    {
        var connectionString = _context.Database.GetConnectionString();
        var csb = new SqlConnectionStringBuilder(connectionString);
        var databaseName = csb.InitialCatalog;
        
        // Проверяем через master, что сервер вообще живой
        csb.InitialCatalog = "master";
        var masterConnectionString = csb.ConnectionString;
        var delay = TimeSpan.FromSeconds(5);
        var attempt = 1;

        _logger.LogInformation("Waiting for SQL Server to be ready (Target: {DatabaseName})...", databaseName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Если смогли подключиться к master — сервер готов
                _logger.LogInformation("SQL Server is responsive after {Attempt} attempts.", attempt);
                return;
            }
            catch (SqlException)
            {
                _logger.LogWarning("SQL Server is not ready yet (Attempt {Attempt}). Retrying in {DelaySeconds}s...", 
                    attempt, delay.TotalSeconds);
            }

            attempt++;
            await Task.Delay(delay, cancellationToken);
        }
    }
}
