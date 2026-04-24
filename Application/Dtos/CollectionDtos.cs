using System.Linq.Expressions;
using Domain;

namespace Application.Dtos;

public record CollectionResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsPublic,
    bool IsSystem,
    int ItemsCount,
    DateTimeOffset CreatedAt)
{
    public static Expression<Func<Collection, CollectionResponse>> Projection => c => new CollectionResponse(
        c.Id,
        c.Name,
        c.Description,
        c.IsPublic,
        c.IsSystem,
        c.Publications.Count,
        c.CreatedAt
    );
}

public record CreateCollectionRequest(
    string Name,
    string? Description,
    bool IsPublic);
