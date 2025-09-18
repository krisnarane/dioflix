using Dioflix.Functions.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Net.Http.Headers;

namespace Dioflix.Functions.Functions;

public class DataStorageFunction
{
    private readonly BlobStorageService _blob;
    public DataStorageFunction(BlobStorageService blob) { _blob = blob; }

    [Function("fnPostDataStorage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dataStorage")] HttpRequestData req,
        FunctionContext ctx)
    {
        if (!req.Headers.TryGetValues("file-type", out var fileTypeValues))
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing 'file-type' header (video|image)");
            return bad;
        }
        var fileType = fileTypeValues.FirstOrDefault() ?? "video";

        if (!req.Headers.TryGetValues("Content-Type", out IEnumerable<string>? ctValues))
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing Content-Type header");
            return bad;
        }

        var contentType = ctValues.First();
        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        if (string.IsNullOrEmpty(mediaType.Boundary.ToString()))
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.UnsupportedMediaType);
            await bad.WriteStringAsync("Content-Type must be multipart/form-data with boundary");
            return bad;
        }

        var reader = new MultipartReader(mediaType.Boundary.ToString(), req.Body);
        string? uploadedUrl = null;
        for (var section = await reader.ReadNextSectionAsync(); section != null; section = await reader.ReadNextSectionAsync())
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
            if (hasContentDispositionHeader && contentDisposition is not null && contentDisposition.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                var fileName = contentDisposition.FileName.Value!;
                uploadedUrl = await _blob.UploadAsync(section.Body, fileName, fileType, ctx.CancellationToken);
                break;
            }
        }

        if (uploadedUrl is null)
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("No file found in multipart body");
            return bad;
        }

        var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new { url = uploadedUrl });
        return ok;
    }
}
