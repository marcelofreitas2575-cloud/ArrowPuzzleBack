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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetProgress_EmptyUserId_Throws(string userId)
    {
        var mediator = CreateMediator();
        await Assert.ThrowsAsync<ArgumentException>(() => mediator.Send(new GetProgressQuery(userId)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CompleteLevel_EmptyUserId_Throws(string userId)
    {
        var mediator = CreateMediator();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            mediator.Send(new CompleteLevelCommand(userId, "001")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CompleteLevel_EmptyLevelId_Throws(string levelId)
    {
        var mediator = CreateMediator();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            mediator.Send(new CompleteLevelCommand("user-1", levelId)));
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
