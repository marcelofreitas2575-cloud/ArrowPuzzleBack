using ArrowPuzzle.PlayerProgress.Application;
using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Application.Progress.CompleteLevel;
using ArrowPuzzle.PlayerProgress.Application.Progress.GetProgress;
using ArrowPuzzle.PlayerProgress.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.PlayerProgress.Application.Tests;

public class ProgressHandlersTests
{
    private static IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddPlayerProgressApplication();
        services.AddSingleton<IPlayerProgressRepository, InMemoryPlayerProgressRepository>();
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task GetProgress_WhenEmpty_ReturnsEmptyCompletedList()
    {
        var mediator = CreateMediator();
        var state = await mediator.Send(new GetProgressQuery("user-1"));

        Assert.Equal("user-1", state.UserId);
        Assert.Empty(state.CompletedLevelIds);
    }

    [Fact]
    public async Task CompleteLevel_ThenGetProgress_IncludesLevel()
    {
        var mediator = CreateMediator();
        await mediator.Send(new CompleteLevelCommand("user-1", "001"));
        var state = await mediator.Send(new GetProgressQuery("user-1"));

        Assert.Contains("001", state.CompletedLevelIds);
    }
}
