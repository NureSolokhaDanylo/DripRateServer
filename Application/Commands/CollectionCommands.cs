using ErrorOr;
using MediatR;

namespace Application.Commands;

public record CreateCollectionCommand(
    Guid UserId,
    string Name,
    string? Description,
    bool IsPublic) : IRequest<ErrorOr<Guid>>;

public record UpdateCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    string Name,
    string? Description,
    bool IsPublic) : IRequest<ErrorOr<Success>>;

public record AddToCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    Guid PublicationId) : IRequest<ErrorOr<Success>>;

public record RemoveFromCollectionCommand(
    Guid UserId,
    Guid CollectionId,
    Guid PublicationId) : IRequest<ErrorOr<Success>>;

public record ToggleLikeCommand(
    Guid UserId,
    Guid PublicationId) : IRequest<ErrorOr<bool>>; // Returns true if liked, false if unliked

public record ToggleSaveCommand(
    Guid UserId,
    Guid PublicationId) : IRequest<ErrorOr<bool>>; // Returns true if saved, false if unsaved
