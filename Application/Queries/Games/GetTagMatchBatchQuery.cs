using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Games;

public record GetTagMatchBatchQuery(
    Guid UserId,
    int BatchSize = 10
) : IRequest<ErrorOr<List<TagMatchGameItemDto>>>;
