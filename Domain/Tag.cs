namespace Domain;

public sealed class Tag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    private Tag() { }

    public Tag(string name, string category)
    {
        Name = name;
        Category = category;
    }
}
