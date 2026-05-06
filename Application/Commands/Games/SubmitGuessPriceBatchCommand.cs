using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands.Games;

public record SubmitGuessPriceBatchCommand(
    Guid UserId,
    List<GuessPriceResultDto> Results
) : IRequest<ErrorOr<Success>>;
