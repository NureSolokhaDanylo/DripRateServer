using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Metrics;

public sealed record GetAarrrMetricsQuery() : IRequest<ErrorOr<AarrrMetricsResponse>>;
