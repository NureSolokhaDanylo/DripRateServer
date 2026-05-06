using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetGuessPriceBatchQuery(
    Guid UserId,
    int BatchSize = 10
) : IRequest<ErrorOr<List<GuessPriceGameItemDto>>>;
