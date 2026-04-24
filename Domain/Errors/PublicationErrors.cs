using ErrorOr;

namespace Domain.Errors;

public static class PublicationErrors
{
    public const string NotFoundCode = "Publication.NotFound";
    public const string ForbiddenCode = "Publication.Forbidden";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The publication was not found.");

    public static Error Forbidden => Error.Forbidden(
        code: ForbiddenCode,
        description: "You do not have permission to modify this publication.");
}
