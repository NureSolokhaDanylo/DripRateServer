using Domain;

namespace Application.Dtos;

public record CreateReportRequest(
    ReportTargetType TargetType,
    Guid TargetId,
    ReportCategory Category,
    string? Text);

public record ReportedEntityDto(
    ReportTargetType TargetType,
    Guid TargetId,
    int PendingReportsCount,
    ReportCategory MostCommonCategory,
    DateTimeOffset FirstReportedAt,
    DateTimeOffset LastReportedAt,
    Guid? AssignedToUserId,
    string? AssignedToUserName);

public record ReportDto(
    Guid Id,
    ReportCategory Category,
    string? Text,
    Guid AuthorId,
    string AuthorDisplayName,
    DateTimeOffset CreatedAt);

public record ResolveReportedEntityRequest(
    ReportTargetType TargetType,
    Guid TargetId,
    ModerationAction Action);
