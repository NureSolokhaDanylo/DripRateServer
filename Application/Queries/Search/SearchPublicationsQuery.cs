using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries.Search;

public record SearchPublicationsQuery(
    string? SearchQuery,
    List<Guid>? TagIds,
    Guid? UserId = null,
    DateTimeOffset? Cursor = null,
    int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;
