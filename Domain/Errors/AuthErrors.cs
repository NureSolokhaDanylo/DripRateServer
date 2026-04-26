using ErrorOr;

namespace Domain.Errors;

public static class AuthErrors
{
    public const string InvalidCredentialsCode = "Auth.InvalidCredentials";
    public const string EmailAlreadyTakenCode = "DuplicateEmail";
    public const string UnauthorizedCode = "Auth.Unauthorized";

    public static Error InvalidCredentials => Error.Unauthorized(
        code: InvalidCredentialsCode,
        description: "Invalid email or password.");

    public static Error EmailAlreadyTaken => Error.Conflict(
        code: EmailAlreadyTakenCode,
        description: "A user with this email already exists.");

    public static Error Unauthorized => Error.Unauthorized(
        code: UnauthorizedCode,
        description: "You must be authenticated to access this resource.");
}
