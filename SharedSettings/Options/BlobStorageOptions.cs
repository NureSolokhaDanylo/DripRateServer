namespace SharedSettings.Options;

public sealed class BlobStorageOptions : IOptions2
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;

    public string GetSectionName() => "BlobStorage";
}
