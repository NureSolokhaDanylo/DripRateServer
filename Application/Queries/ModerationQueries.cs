using Application.Dtos;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetReportedEntitiesQuery(
    int Take = 10) : IRequest<ErrorOr<List<ReportedEntityDto>>>;

public record GetEntityReportsQuery(
    ReportTargetType TargetType,
    Guid TargetId) : IRequest<ErrorOr<List<ReportDto>>>;
