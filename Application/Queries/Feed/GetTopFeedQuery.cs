using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Feed;

public record GetTopFeedQuery(
    Guid UserId,
    TopFeedPeriod Period = TopFeedPeriod.Weekly,
    bool OnlyFollowing = false,
    List<Guid>? TagIds = null,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
