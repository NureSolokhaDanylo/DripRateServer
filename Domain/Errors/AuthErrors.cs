using ErrorOr;

namespace Domain.Errors;

public static class AuthErrors
{
    public const string InvalidCredentialsCode = "Auth.InvalidCredentials";
    public const string EmailAlreadyTakenCode = "DuplicateEmail";
    public const string UserNameAlreadyTakenCode = "DuplicateUserName";

    public static Error InvalidCredentials => Error.Unauthorized(
        code: InvalidCredentialsCode,
        description: "Invalid email/username or password.");

    public static Error EmailAlreadyTaken => Error.Conflict(
        code: EmailAlreadyTakenCode,
        description: "A user with this email already exists.");

    public static Error UserNameAlreadyTaken => Error.Conflict(
        code: UserNameAlreadyTakenCode,
        description: "This username is already taken.");
}
