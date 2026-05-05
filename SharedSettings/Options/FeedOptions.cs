namespace SharedSettings.Options;

public sealed class FeedOptions : IOptions2
{
    public int AdFrequency { get; set; } = 5;
    public int ViewCooldownHours { get; set; } = 24;

    public string GetSectionName() => "Feed";
}
