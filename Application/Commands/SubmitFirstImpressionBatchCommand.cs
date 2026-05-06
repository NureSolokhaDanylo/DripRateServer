using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Commands;

public record SubmitFirstImpressionBatchCommand(
    Guid UserId,
    List<FirstImpressionResultDto> Results
) : IRequest<ErrorOr<Success>>;
