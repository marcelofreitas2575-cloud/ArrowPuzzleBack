using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.BuildingBlocks.Observability;
using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.PlayerProgress.Api.Dapr;
using ArrowPuzzle.PlayerProgress.Api.Grpc;
using ArrowPuzzle.PlayerProgress.Application;
using ArrowPuzzle.PlayerProgress.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddArrowPuzzleOpenTelemetry("arrowpuzzle-player-progress");
builder.ConfigureKestrelForArrowPuzzleService();
builder.Services.AddInternalGrpcIfEnabled(builder.Configuration);

builder.Services.AddPlayerProgressApplication();
builder.Services.AddPlayerProgressInfrastructure(builder.Configuration);
builder.Services.AddArrowPuzzleDaprAppApiToken(builder.Configuration);

var app = builder.Build();

app.UseArrowPuzzleCorrelationId();
app.UseArrowPuzzlePrometheusMetrics();
app.UseArrowPuzzleDaprAppApiToken();
app.MapInternalGrpcService<PlayerProgressGrpcService>();
app.MapPlayerProgressDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "player-progress" }));

app.Run();
