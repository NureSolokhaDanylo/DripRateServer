using ErrorOr;
using MediatR;

namespace Application.Commands.Comments;

public record DeleteCommentCommand(
    Guid UserId,
    Guid CommentId) : IRequest<ErrorOr<Deleted>>;
