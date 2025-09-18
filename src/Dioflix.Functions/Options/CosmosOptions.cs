namespace Dioflix.Functions.Options;

public class CosmosOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = "catalog";
    public string Container { get; set; } = "movies";
    public string PartitionKeyPath { get; set; } = "/id";
}
