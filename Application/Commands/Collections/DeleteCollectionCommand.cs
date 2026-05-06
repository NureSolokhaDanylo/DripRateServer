using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record DeleteCollectionCommand(
    Guid UserId,
    Guid CollectionId) : IRequest<ErrorOr<Deleted>>;
