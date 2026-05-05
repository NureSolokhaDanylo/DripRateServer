using ErrorOr;

namespace Domain.Errors;

public static class AdvertisementErrors
{
    public const string NotFoundCode = "Advertisement.NotFound";
    public const string UnauthorizedCode = "Advertisement.Unauthorized";
    public const string LimitReachedCode = "Advertisement.LimitReached";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "Advertisement not found.");

    public static Error Unauthorized => Error.Forbidden(
        code: UnauthorizedCode,
        description: "You are not authorized to manage advertisements.");

    public static Error LimitReached => Error.Conflict(
        code: LimitReachedCode,
        description: "Cannot activate advertisement because it has reached its impression limit.");
}
