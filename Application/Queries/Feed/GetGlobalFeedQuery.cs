using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Feed;

public record GetGlobalFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<GlobalFeedResponse>>;
