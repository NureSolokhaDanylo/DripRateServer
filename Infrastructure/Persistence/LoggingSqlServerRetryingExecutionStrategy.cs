using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Persistence;

public sealed class LoggingSqlServerRetryingExecutionStrategy : SqlServerRetryingExecutionStrategy
{
    private readonly ILogger _logger;

    public LoggingSqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<int>? errorNumbersToAdd,
        ILogger logger)
        : base(dependencies, maxRetryCount, maxRetryDelay, errorNumbersToAdd)
    {
        _logger = logger;
    }

    protected override bool ShouldRetryOn(Exception? exception)
    {
        if (exception is SqlException sqlEx)
        {
            // Мы логируем каждую попытку ретрая
            _logger.LogWarning("Database connection attempt failed. Error: {Message}. Retrying...", sqlEx.Message);
        }
        return base.ShouldRetryOn(exception);
    }
}
