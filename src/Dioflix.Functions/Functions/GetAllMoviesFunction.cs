using Dioflix.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dioflix.Functions.Functions;

public class GetAllMoviesFunction
{
    private readonly MoviesRepository _repo;
    public GetAllMoviesFunction(MoviesRepository repo) { _repo = repo; }

    [Function("fnGetAllMovies")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "all")] HttpRequestData req,
        FunctionContext ctx)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var token = query["continuationToken"];
        var pageSizeStr = query["pageSize"];
        int pageSize = 20;
        if (!string.IsNullOrEmpty(pageSizeStr) && int.TryParse(pageSizeStr, out var ps) && ps > 0 && ps <= 100)
        {
            pageSize = ps;
        }

        var (items, continuation) = await _repo.ListAsync(token, pageSize, ctx.CancellationToken);
        var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new { items, continuationToken = continuation }, ctx.CancellationToken);
        return ok;
    }
}
