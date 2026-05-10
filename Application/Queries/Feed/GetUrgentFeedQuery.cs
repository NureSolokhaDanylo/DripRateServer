using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Feed;

public record GetUrgentFeedQuery(Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
