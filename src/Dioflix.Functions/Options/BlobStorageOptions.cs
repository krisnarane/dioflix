namespace Dioflix.Functions.Options;

public class BlobStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string VideoContainer { get; set; } = "video";
    public string ImageContainer { get; set; } = "image";
}
