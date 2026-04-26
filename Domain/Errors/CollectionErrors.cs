using ErrorOr;

namespace Domain.Errors;

public static class CollectionErrors
{
    public const string NotFoundCode = "Collection.NotFound";
    public const string ForbiddenCode = "Collection.Forbidden";
    public const string LikesNotInitializedCode = "Collection.LikesNotInitialized";
    public const string NameAlreadyExistsCode = "Collection.NameAlreadyExists";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The collection was not found.");

    public static Error Forbidden => Error.Forbidden(
        code: ForbiddenCode,
        description: "You do not have permission to access or modify this collection.");

    public static Error LikesNotInitialized => Error.Failure(
        code: LikesNotInitializedCode,
        description: "User's internal likes collection is not properly initialized.");

    public static Error NameAlreadyExists => Error.Conflict(
        code: NameAlreadyExistsCode,
        description: "A collection with this name already exists.");
}
