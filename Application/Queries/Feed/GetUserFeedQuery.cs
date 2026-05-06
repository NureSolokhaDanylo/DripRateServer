using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Feed;

public record GetUserFeedQuery(Guid UserId, Guid? CurrentUserId = null, DateTimeOffset? Cursor = null, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
