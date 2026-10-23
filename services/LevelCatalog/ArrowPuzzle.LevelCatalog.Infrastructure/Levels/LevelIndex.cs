namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

internal static class LevelIndex
{
    public static bool TryParse(string levelId, out int index)
    {
        index = 0;
        if (string.IsNullOrWhiteSpace(levelId))
        {
            return false;
        }

        var trimmed = levelId.Trim();
        if (trimmed.Length >= 1 && trimmed.All(char.IsDigit))
        {
            return int.TryParse(trimmed, out index) && index >= 1;
        }

        return false;
    }

    public static string Format(int index) => index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
}
