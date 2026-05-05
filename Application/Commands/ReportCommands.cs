using Application.Dtos;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Commands;

public record CreateReportCommand(
    Guid AuthorId,
    ReportTargetType TargetType,
    Guid TargetId,
    ReportCategory Category,
    string? Text) : IRequest<ErrorOr<Success>>;

public record AssignReportedEntityCommand(
    Guid ModeratorId,
    ReportTargetType TargetType,
    Guid TargetId) : IRequest<ErrorOr<Success>>;

public record ResolveReportedEntityCommand(
    Guid ModeratorId,
    ReportTargetType TargetType,
    Guid TargetId,
    ModerationAction Action) : IRequest<ErrorOr<Success>>;
