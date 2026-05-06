using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record UpdateCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    string Name,
    string? Description,
    bool IsPublic) : IRequest<ErrorOr<Success>>;
