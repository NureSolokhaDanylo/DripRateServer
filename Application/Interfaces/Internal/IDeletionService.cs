namespace Application.Interfaces.Internal;

internal interface IDeletionService
{
    Task DeleteUserContentAsync(Guid userId, CancellationToken cancellationToken);
    Task DeletePublicationContentAsync(Guid publicationId, CancellationToken cancellationToken);
    Task DeleteClothContentAsync(Guid clothId, CancellationToken cancellationToken);
}
