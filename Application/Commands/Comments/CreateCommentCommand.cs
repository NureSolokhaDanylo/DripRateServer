using ErrorOr;
using MediatR;

namespace Application.Commands.Comments;

public record CreateCommentCommand(
    Guid UserId,
    Guid PublicationId,
    string Text,
    Guid? ParentCommentId) : IRequest<ErrorOr<Guid>>;
