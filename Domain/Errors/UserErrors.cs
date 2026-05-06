using ErrorOr;

namespace Domain.Errors;

public static class UserErrors
{
    public const string NotFoundCode = "User.NotFound";

    public const string CannotBanModeratorCode = "User.CannotBanModerator";

    public static Error CannotBanModerator => Error.Conflict(
        code: CannotBanModeratorCode,
        description: "Moderator account cannot be banned.");

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "The requested user was not found.");
}
