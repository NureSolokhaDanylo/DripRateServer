using Application.Dtos;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Commands.Reports;

public record ResolveReportedEntityCommand(
    Guid ModeratorId,
    ReportTargetType TargetType,
    Guid TargetId,
    ModerationAction Action) : IRequest<ErrorOr<Success>>;
