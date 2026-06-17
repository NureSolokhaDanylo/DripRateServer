namespace Domain;

public sealed class AdvertisementView
{
    private Guid _advertisementId;
    private Guid _userId;
    private DateTimeOffset _viewedAt;

    private Advertisement _advertisement = null!;
    private User _user = null!;

    public Guid AdvertisementId => _advertisementId;
    public Guid UserId => _userId;
    public DateTimeOffset ViewedAt => _viewedAt;

    public Advertisement Advertisement => _advertisement;
    public User User => _user;

    private AdvertisementView() { }

    public AdvertisementView(Guid advertisementId, Guid userId)
    {
        if (advertisementId == Guid.Empty) throw new ArgumentException("Advertisement ID cannot be empty.", nameof(advertisementId));
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        _advertisementId = advertisementId;
        _userId = userId;
        _viewedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateViewedAt()
    {
        _viewedAt = DateTimeOffset.UtcNow;
    }
}
