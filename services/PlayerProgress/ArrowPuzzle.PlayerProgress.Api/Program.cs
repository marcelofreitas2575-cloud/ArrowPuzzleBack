using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.PlayerProgress.Api.Dapr;
using ArrowPuzzle.PlayerProgress.Api.Grpc;
using ArrowPuzzle.PlayerProgress.Application;
using ArrowPuzzle.PlayerProgress.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelForInsecureGrpc();

builder.Services.AddGrpc();
builder.Services.AddPlayerProgressApplication();
builder.Services.AddPlayerProgressInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<PlayerProgressGrpcService>();
app.MapPlayerProgressDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "player-progress" }));

app.Run();
