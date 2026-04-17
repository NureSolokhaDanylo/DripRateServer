namespace Domain;

public sealed class Cloth
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? StoreLink { get; private set; }
    public decimal? EstimatedPrice { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private Cloth() { }

    public Cloth(Guid userId, string name, string? brand = null)
    {
        UserId = userId;
        Name = name;
        Brand = brand;
    }

    public void UpdateInfo(string name, string? brand, string? photoUrl, string? storeLink, decimal? price)
    {
        Name = name;
        Brand = brand;
        PhotoUrl = photoUrl;
        StoreLink = storeLink;
        EstimatedPrice = price;
    }
}
