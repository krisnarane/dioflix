using Dioflix.Functions.Domain;
using Dioflix.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dioflix.Functions.Functions;

public class PostMovieFunction
{
    private readonly MoviesRepository _repo;
    public PostMovieFunction(MoviesRepository repo) { _repo = repo; }

    [Function("fnPostDatabase")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movie")] HttpRequestData req,
        FunctionContext ctx)
    {
        var movie = await req.ReadFromJsonAsync<Movie>(cancellationToken: ctx.CancellationToken);
        if (movie is null || string.IsNullOrWhiteSpace(movie.Id) || string.IsNullOrWhiteSpace(movie.Title))
        {
            var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid body. Required: id, title, year, video, thumb");
            return bad;
        }

        var saved = await _repo.UpsertAsync(movie, ctx.CancellationToken);
        var created = req.CreateResponse(System.Net.HttpStatusCode.Created);
        await created.WriteAsJsonAsync(saved, ctx.CancellationToken);
        return created;
    }
}
