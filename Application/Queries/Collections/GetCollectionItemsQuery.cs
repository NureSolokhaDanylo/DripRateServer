using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Collections;

public record GetCollectionItemsQuery(Guid CollectionId, Guid UserId, DateTimeOffset? Cursor, int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
