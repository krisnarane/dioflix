using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dioflix.Functions.Options;
using Microsoft.Extensions.Options;

namespace Dioflix.Functions.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _options;

    public BlobStorageService(IOptions<BlobStorageOptions> options)
    {
        _options = options.Value;
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string fileType, CancellationToken ct)
    {
        var containerName = fileType?.ToLowerInvariant() == "image" ? _options.ImageContainer : _options.VideoContainer;
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid():N}-{fileName}";
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, overwrite: true, cancellationToken: ct);
        return blob.Uri.ToString();
    }
}
