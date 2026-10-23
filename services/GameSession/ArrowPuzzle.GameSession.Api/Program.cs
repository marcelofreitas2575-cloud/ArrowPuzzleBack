using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.GameSession.Api.Dapr;
using ArrowPuzzle.GameSession.Api.Grpc;
using ArrowPuzzle.GameSession.Application;
using ArrowPuzzle.GameSession.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelForInsecureGrpc();

builder.Services.AddGrpc();
builder.Services.AddGameSessionApplication();
builder.Services.AddGameSessionInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<GameSessionGrpcService>();
app.MapGameSessionDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "game-session" }));

app.Run();
