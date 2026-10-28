using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.BuildingBlocks.Observability;
using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.LevelCatalog.Api.Dapr;
using ArrowPuzzle.LevelCatalog.Api.Grpc;
using ArrowPuzzle.LevelCatalog.Application;
using ArrowPuzzle.LevelCatalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddArrowPuzzleOpenTelemetry("arrowpuzzle-level-catalog");
builder.ConfigureKestrelForArrowPuzzleService();
builder.Services.AddInternalGrpcIfEnabled(builder.Configuration);

builder.Services.AddLevelCatalogApplication();
builder.Services.AddLevelCatalogInfrastructure(builder.Configuration);
builder.Services.AddArrowPuzzleDaprAppApiToken(builder.Configuration);

var app = builder.Build();

app.UseArrowPuzzleCorrelationId();
app.UseArrowPuzzlePrometheusMetrics();
app.UseArrowPuzzleDaprAppApiToken();
app.MapInternalGrpcService<LevelCatalogGrpcService>();
app.MapLevelCatalogDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "level-catalog" }));

app.Run();
