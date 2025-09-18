using Dioflix.Functions.Domain;
using Dioflix.Functions.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Dioflix.Functions.Services;

public class MoviesRepository
{
    private readonly CosmosClient _client;
    private readonly Container _container;

    public MoviesRepository(IOptions<CosmosOptions> options)
    {
        var opt = options.Value;
        _client = new CosmosClient(opt.ConnectionString);
        var db = _client.CreateDatabaseIfNotExistsAsync(opt.Database).GetAwaiter().GetResult();
        var containerResponse = db.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            Id = opt.Container,
            PartitionKeyPath = opt.PartitionKeyPath
        }).GetAwaiter().GetResult();
        _container = containerResponse.Container;
    }

    public async Task<Movie> UpsertAsync(Movie movie, CancellationToken ct)
    {
        movie.UpdatedAt = DateTimeOffset.UtcNow;
        if (movie.CreatedAt == default)
            movie.CreatedAt = DateTimeOffset.UtcNow;

        var pk = new PartitionKey(movie.Id);
        var resp = await _container.UpsertItemAsync(movie, pk, cancellationToken: ct);
        return resp.Resource;
    }

    public async Task<Movie?> GetByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var resp = await _container.ReadItemAsync<Movie>(id, new PartitionKey(id), cancellationToken: ct);
            return resp.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<(IReadOnlyList<Movie> Items, string? ContinuationToken)> ListAsync(string? continuationToken, int pageSize, CancellationToken ct)
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");
        var iterator = _container.GetItemQueryIterator<Movie>(query, continuationToken, new QueryRequestOptions
        {
            MaxItemCount = pageSize
        });

        if (!iterator.HasMoreResults)
            return (Array.Empty<Movie>(), null);

        var page = await iterator.ReadNextAsync(ct);
        return (page.Resource.ToList(), page.ContinuationToken);
    }
}
