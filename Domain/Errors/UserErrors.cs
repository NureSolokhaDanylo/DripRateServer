using ErrorOr;

namespace Domain.Errors;

public static class UserErrors
{
    public const string NotFoundCode = "User.NotFound";

    public const string DeleteFailedCode = "User.DeleteFailed";

    public static Error DeleteFailed => Error.Failure(
        code: DeleteFailedCode,
        description: "Failed to delete user account.");

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The requested user was not found.");
}
