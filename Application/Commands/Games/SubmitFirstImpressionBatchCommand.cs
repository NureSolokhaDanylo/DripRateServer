using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands.Games;

public record SubmitFirstImpressionBatchCommand(
    Guid UserId,
    List<FirstImpressionResultDto> Results
) : IRequest<ErrorOr<Success>>;
