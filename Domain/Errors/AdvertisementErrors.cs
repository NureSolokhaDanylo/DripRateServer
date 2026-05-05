using ErrorOr;

namespace Domain.Errors;

public static class AdvertisementErrors
{
    public static Error NotFound => Error.NotFound(
        code: "Advertisement.NotFound",
        description: "Advertisement not found.");

    public static Error Unauthorized => Error.Forbidden(
        code: "Advertisement.Unauthorized",
        description: "You are not authorized to manage advertisements.");
}
