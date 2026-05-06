using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetFirstImpressionBatchQuery(
    Guid UserId,
    int BatchSize = 10
) : IRequest<ErrorOr<List<FirstImpressionGameItemDto>>>;
