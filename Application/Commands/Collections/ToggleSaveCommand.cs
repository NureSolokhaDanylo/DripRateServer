using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record ToggleSaveCommand(
    Guid UserId,
    Guid PublicationId) : IRequest<ErrorOr<bool>>; // Returns true if saved, false if unsaved
