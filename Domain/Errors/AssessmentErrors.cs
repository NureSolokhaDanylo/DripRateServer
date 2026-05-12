using ErrorOr;

namespace Domain.Errors;

public static class AssessmentErrors
{
    public const string CannotRateOwnPublicationCode = "Assessment.CannotRateOwnPublication";
    public const string NotFoundCode = "Assessment.NotFound";

    public static Error CannotRateOwnPublication => Error.Conflict(
        code: CannotRateOwnPublicationCode,
        description: "You cannot rate your own publication.");

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "User has not assessed this publication.");
}
