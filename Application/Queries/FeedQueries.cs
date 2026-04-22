using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetGlobalFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public record GetSubscriptionFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public record GetUserFeedQuery(string Username, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
