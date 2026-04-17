using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly MyDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(MyDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Database initialization starting...");
        
        // EF Core сам будет бесконечно пытаться подключиться
        // благодаря нашей LoggingSqlServerRetryingExecutionStrategy
        await _context.Database.MigrateAsync(cancellationToken);
        
        _logger.LogInformation("Database initialization completed successfully.");
    }
}
