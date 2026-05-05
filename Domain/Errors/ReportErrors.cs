using ErrorOr;

namespace Domain.Errors;

public static class ReportErrors
{
    public const string NotFoundCode = "Report.NotFound";
    public const string AlreadyAssignedCode = "Report.AlreadyAssigned";
    public const string InvalidTargetCode = "Report.InvalidTarget";
    public const string SelfReportCode = "Report.SelfReport";
    public const string DuplicateReportCode = "Report.DuplicateReport";
    public const string UnauthorizedCode = "Report.Unauthorized";

    public static Error NotFound => Error.NotFound(
        code: NotFoundCode,
        description: "Reports for specified entity not found or already resolved.");

    public static Error AlreadyAssigned => Error.Conflict(
        code: AlreadyAssignedCode,
        description: "These reports are already being reviewed by another moderator.");
    
    public static Error InvalidTarget => Error.Validation(
        code: InvalidTargetCode,
        description: "The reported entity does not exist.");

    public static Error SelfReport => Error.Conflict(
        code: SelfReportCode,
        description: "You cannot report your own content.");

    public static Error DuplicateReport => Error.Conflict(
        code: DuplicateReportCode,
        description: "You have already reported this entity and it is still under review.");

    public static Error Unauthorized => Error.Forbidden(
        code: UnauthorizedCode,
        description: "You are not authorized to resolve these reports.");
}
