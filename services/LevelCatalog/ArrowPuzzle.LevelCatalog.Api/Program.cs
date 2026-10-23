using ArrowPuzzle.BuildingBlocks.Hosting;
using ArrowPuzzle.LevelCatalog.Api.Dapr;
using ArrowPuzzle.LevelCatalog.Api.Grpc;
using ArrowPuzzle.LevelCatalog.Application;
using ArrowPuzzle.LevelCatalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelForInsecureGrpc();

builder.Services.AddGrpc();
builder.Services.AddLevelCatalogApplication();
builder.Services.AddLevelCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<LevelCatalogGrpcService>();
app.MapLevelCatalogDaprInvoke();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "level-catalog" }));

app.Run();
