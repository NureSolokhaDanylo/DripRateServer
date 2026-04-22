using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetCommentsQuery(
    Guid PublicationId,
    Guid? UserId,
    DateTimeOffset? Cursor,
    int Take = 30) : IRequest<ErrorOr<List<CommentResponse>>>;
