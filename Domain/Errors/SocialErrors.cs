using ErrorOr;

namespace Domain.Errors;

public static class SocialErrors
{
    public const string CannotFollowSelfCode = "Social.CannotFollowSelf";

    public static Error CannotFollowSelf => Error.Validation(
        code: CannotFollowSelfCode,
        description: "You cannot follow your own profile.");
}
