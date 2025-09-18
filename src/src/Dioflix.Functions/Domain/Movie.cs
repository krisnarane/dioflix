namespace Dioflix.Functions.Domain;

public class Movie
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Year { get; set; } = default!;
    public string Video { get; set; } = default!;
    public string Thumb { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
