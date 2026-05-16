using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands.Games;

public record SubmitTagMatchBatchCommand(
    Guid UserId,
    List<TagMatchResultDto> Results
) : IRequest<ErrorOr<List<TagMatchResultResponse>>>;
