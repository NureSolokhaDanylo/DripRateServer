using Application.Dtos;
using Domain;
using ErrorOr;
using MediatR;

namespace Application.Queries.Moderation;

public record GetReportedEntitiesQuery(
    int Take = 10) : IRequest<ErrorOr<List<ReportedEntityDto>>>;
