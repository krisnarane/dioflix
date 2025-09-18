using Dioflix.Functions.Options;
using Dioflix.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddOptions<BlobStorageOptions>()
        .Configure<IConfiguration>((opt, cfg) =>
        {
            opt.ConnectionString = cfg["BlobStorage:ConnectionString"] ?? cfg["AzureWebJobsStorage"] ?? string.Empty;
            opt.VideoContainer = cfg["BlobStorage:Containers:Video"] ?? "video";
            opt.ImageContainer = cfg["BlobStorage:Containers:Image"] ?? "image";
        })
    .Services
    .AddOptions<CosmosOptions>()
        .Configure<IConfiguration>((opt, cfg) =>
        {
            opt.ConnectionString = cfg["MoviesCosmos:ConnectionString"] ?? string.Empty;
            opt.Database = cfg["MoviesCosmos:Database"] ?? "catalog";
            opt.Container = cfg["MoviesCosmos:Container"] ?? "movies";
            opt.PartitionKeyPath = cfg["MoviesCosmos:PartitionKeyPath"] ?? "/id";
        })
    .Services
    .AddSingleton<BlobStorageService>()
    .AddSingleton<MoviesRepository>();

builder.Build().Run();
