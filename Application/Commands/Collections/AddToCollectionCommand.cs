using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record AddToCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    Guid PublicationId) : IRequest<ErrorOr<Success>>;
