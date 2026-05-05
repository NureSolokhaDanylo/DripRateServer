using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetGlobalFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<GlobalFeedResponse>>;

public record GetSubscriptionFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public record GetUserFeedQuery(Guid UserId, Guid? CurrentUserId = null, DateTimeOffset? Cursor = null, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public enum TopFeedPeriod
{
    AllTime,
    Weekly,
    Monthly
}

public record GetTopFeedQuery(
    Guid UserId,
    TopFeedPeriod Period = TopFeedPeriod.Weekly,
    bool OnlyFollowing = false,
    List<Guid>? TagIds = null,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
