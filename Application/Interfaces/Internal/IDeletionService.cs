namespace Application.Interfaces.Internal;

internal interface IDeletionService
{
    Task DeletePublicationContentAsync(Guid publicationId, CancellationToken cancellationToken);
    Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken);
}