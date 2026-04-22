using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record GetMyCollectionsQuery(Guid UserId) : IRequest<ErrorOr<List<CollectionResponse>>>;

public record GetCollectionItemsQuery(Guid CollectionId, Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
