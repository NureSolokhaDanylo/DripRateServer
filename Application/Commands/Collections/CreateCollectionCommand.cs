using ErrorOr;
using MediatR;

namespace Application.Commands.Collections;

public record CreateCollectionCommand(
    Guid UserId,
    string Name,
    string? Description,
    bool IsPublic) : IRequest<ErrorOr<Guid>>;
