using ErrorOr;

namespace Domain.Errors;

public static class CommentErrors
{
    public const string NotFoundCode = "Comment.NotFound";
    public const string ParentNotFoundCode = "Comment.ParentNotFound";
    public const string ForbiddenCode = "Comment.Forbidden";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The comment was not found.");

    public static Error ParentNotFound => Error.NotFound(
        code: ParentNotFoundCode,
        description: "The parent comment you are replying to was not found.");

    public static Error Forbidden => Error.Forbidden(
        code: ForbiddenCode,
        description: "You do not have permission to modify this comment.");
}
