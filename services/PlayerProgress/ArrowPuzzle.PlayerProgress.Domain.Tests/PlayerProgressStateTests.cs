using Xunit;

namespace ArrowPuzzle.PlayerProgress.Domain.Tests;

public class PlayerProgressStateTests
{
    [Fact]
    public void WithLevelCompleted_AddsIdOnce()
    {
        var state = new PlayerProgressState("user-1", Array.Empty<string>());
        var afterFirst = state.WithLevelCompleted("001");
        var afterSecond = afterFirst.WithLevelCompleted("001");

        Assert.Single(afterFirst.CompletedLevelIds);
        Assert.Equal("001", afterFirst.CompletedLevelIds[0]);
        Assert.Same(afterFirst, afterSecond);
    }
}
