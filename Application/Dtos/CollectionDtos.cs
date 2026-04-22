namespace Application.Dtos;

public record CollectionResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsPublic,
    bool IsSystem,
    int ItemsCount,
    DateTimeOffset CreatedAt);

public record CreateCollectionRequest(
    string Name,
    string? Description,
    bool IsPublic);
