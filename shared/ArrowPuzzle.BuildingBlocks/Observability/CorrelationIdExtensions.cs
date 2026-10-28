using Microsoft.AspNetCore.Builder;

namespace ArrowPuzzle.BuildingBlocks.Observability;

public static class CorrelationIdExtensions
{
    public static WebApplication UseArrowPuzzleCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
