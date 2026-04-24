namespace Domain;

public sealed class Tag
{
    private Guid _id;
    private string _name = string.Empty;
    private string _category = string.Empty;

    public Guid Id => _id;
    public string Name => _name;
    public string Category => _category;

    private Tag() { }

    public Tag(string name, string category)
    {
        _name = name;
        _category = category;
    }
}
