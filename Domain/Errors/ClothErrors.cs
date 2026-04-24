using ErrorOr;

namespace Domain.Errors;

public static class ClothErrors
{
    public const string NotFoundCode = "Cloth.NotFound";
    public const string ForbiddenCode = "Cloth.Forbidden";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The requested clothing item was not found in your wardrobe.");

    public static Error Forbidden => Error.Forbidden(
        code: ForbiddenCode,
        description: "You do not have permission to modify this clothing item.");
}
