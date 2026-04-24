using System.Linq.Expressions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions;

public static class QueryableExtensions
{
    public static async Task<ErrorOr<T>> GetByIdOrErrorAsync<T>(
        this IQueryable<T> query,
        Guid id,
        CancellationToken cancellationToken,
        string? errorDescription = null) where T : class
    {
        // We use reflection to find the Id property, assuming standard naming convention
        // For more robustness, one could use EF Core metadata here
        var entity = await query.FirstOrDefaultAsync(CreateIdPredicate<T>(id), cancellationToken);

        if (entity == null)
        {
            return Error.NotFound(description: errorDescription ?? $"{typeof(T).Name} not found.");
        }

        return entity;
    }

    public static async Task<ErrorOr<T>> GetOwnedOrErrorAsync<T>(
        this IQueryable<T> query,
        Guid id,
        Guid userId,
        CancellationToken cancellationToken,
        string? errorDescription = null) where T : class
    {
        var entity = await query.FirstOrDefaultAsync(CreateOwnedPredicate<T>(id, userId), cancellationToken);

        if (entity == null)
        {
            return Error.NotFound(description: errorDescription ?? $"{typeof(T).Name} not found or access denied.");
        }

        return entity;
    }

    private static Expression<Func<T, bool>> CreateIdPredicate<T>(Guid id)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var body = Expression.Equal(property, constant);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> CreateOwnedPredicate<T>(Guid id, Guid userId)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var idProperty = Expression.Property(parameter, "Id");
        var userIdProperty = Expression.Property(parameter, "UserId");
        
        var idConstant = Expression.Constant(id);
        var userIdConstant = Expression.Constant(userId);
        
        var idEqual = Expression.Equal(idProperty, idConstant);
        var userIdEqual = Expression.Equal(userIdProperty, userIdConstant);
        
        var body = Expression.AndAlso(idEqual, userIdEqual);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
