using ErrorOr;

namespace Domain.Errors;

public static class FileErrors
{
    public const string ProcessingFailedCode = "File.ProcessingFailed";

    public static Error ProcessingFailed => Error.Failure(
        code: ProcessingFailedCode,
        description: "Failed to process the uploaded file. Please try again later.");
}
