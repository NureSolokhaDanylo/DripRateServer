using ErrorOr;
using MediatR;

namespace Application.Commands;

public record CreateCommentCommand(
    Guid UserId,
    Guid PublicationId,
    string Text,
    Guid? ParentCommentId) : IRequest<ErrorOr<Guid>>;

public record ToggleCommentLikeCommand(
    Guid UserId,
    Guid CommentId) : IRequest<ErrorOr<bool>>;

public record DeleteCommentCommand(
    Guid UserId,
    Guid CommentId) : IRequest<ErrorOr<Deleted>>;
