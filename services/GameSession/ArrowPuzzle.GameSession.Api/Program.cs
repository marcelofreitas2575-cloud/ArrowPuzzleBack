using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.BuildingBlocks.Observability;
using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.GameSession.Api.Dapr;
using ArrowPuzzle.GameSession.Api.Grpc;
using ArrowPuzzle.GameSession.Application;
using ArrowPuzzle.GameSession.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddArrowPuzzleOpenTelemetry("arrowpuzzle-game-session");
builder.ConfigureKestrelForArrowPuzzleService();
builder.Services.AddInternalGrpcIfEnabled(builder.Configuration);

builder.Services.AddGameSessionApplication();
builder.Services.AddGameSessionInfrastructure(builder.Configuration);
builder.Services.AddArrowPuzzleDaprAppApiToken(builder.Configuration);

var app = builder.Build();

app.UseArrowPuzzleCorrelationId();
app.UseArrowPuzzlePrometheusMetrics();
app.UseArrowPuzzleDaprAppApiToken();
app.MapInternalGrpcService<GameSessionGrpcService>();
app.MapGameSessionDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "game-session" }));

app.Run();
