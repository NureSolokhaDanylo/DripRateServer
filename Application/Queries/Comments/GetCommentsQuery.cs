using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Comments;

public record GetCommentsQuery(
    Guid PublicationId,
    Guid? UserId,
    Guid? ParentCommentId = null,
    DateTimeOffset? Cursor = null,
    int Take = 30) : IRequest<ErrorOr<List<CommentResponse>>>;
