using ArrowPuzzle.Gateway.Api.Authentication;
using ArrowPuzzle.Gateway.Api.Endpoints;
using ArrowPuzzle.Gateway.Application;
using ArrowPuzzle.Gateway.Infrastructure;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayJwtAuthentication(builder.Configuration);
builder.Services.AddGatewayApplication();
builder.Services.AddGatewayInfrastructure(builder.Configuration);
builder.Services.AddHttpClient(nameof(DevAuthEndpoints));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway" }));
app.MapGet(
        "/api/v1/product",
        (IOptions<ProductOptions> product) =>
            Results.Ok(new { displayName = product.Value.DisplayName }))
    .AllowAnonymous();
app.MapDevAuthEndpoints();
app.MapArrowPuzzleApi();

app.Run();
