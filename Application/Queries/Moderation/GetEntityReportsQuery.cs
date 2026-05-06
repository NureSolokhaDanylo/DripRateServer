using Application.Dtos;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Queries.Moderation;

public record GetEntityReportsQuery(
    ReportTargetType TargetType,
    Guid TargetId) : IRequest<ErrorOr<List<ReportDto>>>;
