using ErrorOr;

namespace Domain.Errors;

public static class ReportErrors
{
    public const string NotFoundCode = "Report.NotFound";
    public const string AlreadyAssignedCode = "Report.AlreadyAssigned";
    public const string InvalidTargetCode = "Report.InvalidTarget";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "Reports for specified entity not found or already resolved.");

    public static Error AlreadyAssigned => Error.Conflict(
        code: AlreadyAssignedCode,
        description: "These reports are already being reviewed by another moderator.");
    
    public static Error InvalidTarget => Error.Validation(
        code: InvalidTargetCode,
        description: "The reported entity does not exist.");
}
