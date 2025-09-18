using Dioflix.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dioflix.Functions.Functions;

public class GetMovieDetailFunction
{
    private readonly MoviesRepository _repo;
    public GetMovieDetailFunction(MoviesRepository repo) { _repo = repo; }

    [Function("fnGetMovieDetail")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "detail")] HttpRequestData req,
        FunctionContext ctx)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var id = query["id"];
        if (string.IsNullOrWhiteSpace(id))
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Query param 'id' is required");
            return bad;
        }

        var movie = await _repo.GetByIdAsync(id, ctx.CancellationToken);
        if (movie is null)
        {
            var notFound = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Not found");
            return notFound;
        }

        var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(movie, ctx.CancellationToken);
        return ok;
    }
}
