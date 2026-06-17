namespace Domain;

public sealed class CollectionPublication
{
    private Guid _collectionId;
    private Guid _publicationId;
    private DateTimeOffset _addedAt;

    private Collection _collection = null!;
    private Publication _publication = null!;

    public Guid CollectionId => _collectionId;
    public Guid PublicationId => _publicationId;
    public DateTimeOffset AddedAt => _addedAt;

    public Collection Collection => _collection;
    public Publication Publication => _publication;

    private CollectionPublication() { }

    public CollectionPublication(Guid collectionId, Guid publicationId)
    {
        if (collectionId == Guid.Empty) throw new ArgumentException("Collection ID cannot be empty.", nameof(collectionId));
        if (publicationId == Guid.Empty) throw new ArgumentException("Publication ID cannot be empty.", nameof(publicationId));

        _collectionId = collectionId;
        _publicationId = publicationId;
        _addedAt = DateTimeOffset.UtcNow;
    }
}
