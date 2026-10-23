using ArrowPuzzle.LevelCatalog.Application;
using ArrowPuzzle.LevelCatalog.Application.Levels.GetLevel;
using ArrowPuzzle.LevelCatalog.Application.Ports;
using ArrowPuzzle.LevelCatalog.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.LevelCatalog.Application.Tests;

public class GetLevelHandlerTests
{
    [Fact]
    public async Task GetLevel_WhenRepositoryHasLevel_ReturnsDefinition()
    {
        var expected = new LevelDefinition(LevelId.From("001"), 5, 5, Array.Empty<LevelArrowDefinition>());
        var mediator = CreateMediator(new FakeRepository(expected));

        var result = await mediator.Send(new GetLevelQuery(LevelId.From("001")));

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetLevel_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var mediator = CreateMediator(new FakeRepository(null));

        var result = await mediator.Send(new GetLevelQuery(LevelId.From("999")));

        Assert.Null(result);
    }

    private static IMediator CreateMediator(ILevelCatalogRepository repository)
    {
        var services = new ServiceCollection();
        services.AddLevelCatalogApplication();
        services.AddSingleton(repository);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private sealed class FakeRepository(LevelDefinition? level) : ILevelCatalogRepository
    {
        public Task<LevelDefinition?> GetByIdAsync(LevelId levelId, CancellationToken cancellationToken = default) =>
            Task.FromResult(level);
    }
}
