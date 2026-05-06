using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record ToggleLikeCommand(
    Guid UserId,
    Guid PublicationId) : IRequest<ErrorOr<bool>>; // Returns true if liked, false if unliked
