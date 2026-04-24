namespace Domain;

public sealed class Cloth
{
    private Guid _id;
    private string _name = string.Empty;
    private string? _brand;
    private string? _photoUrl;
    private string? _storeLink;
    private decimal? _estimatedPrice;
    
    private Guid _userId;
    private User _user = null!;

    public Guid Id => _id;
    public string Name => _name;
    public string? Brand => _brand;
    public string? PhotoUrl => _photoUrl;
    public string? StoreLink => _storeLink;
    public decimal? EstimatedPrice => _estimatedPrice;
    
    public Guid UserId => _userId;
    public User User => _user;

    private Cloth() { }

    public Cloth(Guid userId, string name, string? brand = null)
    {
        _userId = userId;
        _name = name;
        _brand = brand;
    }

    public void UpdateInfo(string name, string? brand, string? photoUrl, string? storeLink, decimal? price)
    {
        _name = name;
        _brand = brand;
        _photoUrl = photoUrl;
        _storeLink = storeLink;
        _estimatedPrice = price;
    }
}
