using Microsoft.EntityFrameworkCore;
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
        bool willRetry = base.ShouldRetryOn(exception);
        
        if (willRetry && exception is SqlException sqlEx)
        {
            _logger.LogWarning("Database transient error detected. Error: {Message}. EF Core will retry automatically...", sqlEx.Message);
        }
        
        return willRetry;
    }
}
