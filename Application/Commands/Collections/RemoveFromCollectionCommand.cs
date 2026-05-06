using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record RemoveFromCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    Guid PublicationId) : IRequest<ErrorOr<Success>>;
