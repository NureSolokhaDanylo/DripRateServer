using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Comments;

public record GetCommentByIdQuery(
    Guid CommentId,
    Guid? UserId) : IRequest<ErrorOr<CommentResponse>>;
