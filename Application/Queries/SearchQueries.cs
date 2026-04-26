using Application.Dtos;
using ErrorOr;
using MediatR;

namespace Application.Queries;

public record SearchPublicationsQuery(
    string? SearchQuery,
    List<Guid>? TagIds,
    Guid? UserId = null,
    DateTimeOffset? Cursor = null,
    int Take = 20) : IRequest<ErrorOr<List<PublicationResponse>>>;

public record SearchCollectionsQuery(
    string SearchQuery,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<CollectionResponse>>>;

public record SearchUsersQuery(
    string SearchQuery,
    int Skip = 0,
    int Take = 20) : IRequest<ErrorOr<List<UserProfileResponse>>>;
