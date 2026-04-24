using ErrorOr;

namespace Domain.Errors;

public static class AssessmentErrors
{
    public const string CannotRateOwnPublicationCode = "Assessment.CannotRateOwnPublication";

    public static Error CannotRateOwnPublication => Error.Validation(
        code: CannotRateOwnPublicationCode,
        description: "You cannot rate your own publication.");
}
