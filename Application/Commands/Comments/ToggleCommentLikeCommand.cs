using ErrorOr;
using MediatR;

namespace Application.Commands.Comments;

public record ToggleCommentLikeCommand(
    Guid UserId,
    Guid CommentId) : IRequest<ErrorOr<bool>>;
