using ErrorOr;

namespace Domain.Errors;

public static class UserErrors
{
    public const string NotFoundCode = "User.NotFound";
    public const string ForbiddenCode = "User.Forbidden";

    public const string DeleteFailedCode = "User.DeleteFailed";

    public static Error DeleteFailed => Error.Failure(
        code: DeleteFailedCode,
        description: "Failed to delete user account.");

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The requested user was not found.");

    public static Error Forbidden => Error.Forbidden(
        code: ForbiddenCode,
        description: "You do not have permission to perform this action on this user.");
}
