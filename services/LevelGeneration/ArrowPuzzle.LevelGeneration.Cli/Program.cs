using ArrowPuzzle.LevelGeneration;

if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
{
    PrintHelp();
    return 0;
}

var levelIndex = ParseIntArg(args, "--level", defaultValue: 2);
var seed = ParseIntArg(args, "--seed", defaultValue: levelIndex * 103 + 51);
var maxRetries = ParseIntArg(args, "--retries", defaultValue: 50);
var levelId = ParseStringArg(args, "--id") ?? levelIndex.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
var outputPath = ParseStringArg(args, "--output");

var generator = new ReverseLevelGenerator();
GeneratedLevel? generated = null;

for (var attempt = 0; attempt < maxRetries; attempt++)
{
    var parameters = LevelGenerationParameters.ForLevelIndex(
        levelIndex,
        seed + attempt,
        LevelGenerationDefaults.DefaultTailSafety);

    generated = generator.TryGenerate(parameters);
    if (generated is not null)
    {
        break;
    }
}

if (generated is null)
{
    Console.Error.WriteLine("Falha ao gerar nível solvável. Tente outro --seed ou --level.");
    return 1;
}

if (!PuzzleSolver.IsSolvable(generated.Board))
{
    Console.Error.WriteLine("Gerado mas solver falhou (não deveria acontecer).");
    return 2;
}

var json = LevelJsonExporter.ToJson(levelId, generated.Board);

if (string.IsNullOrWhiteSpace(outputPath))
{
    Console.WriteLine(json);
}
else
{
    var fullPath = Path.GetFullPath(outputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllText(fullPath, json);
    Console.WriteLine($"OK: {fullPath} ({generated.Board.Arrows.Count} setas, seed efetivo ~{seed})");
}

return 0;

static void PrintHelp()
{
    Console.WriteLine("""
        Gera um nível Arrow Puzzle (offline, solvável). Estratégia 3.4: Backtrack (padrão).

        Uso:
          dotnet run --project services/LevelGeneration/ArrowPuzzle.LevelGeneration.Cli -- --level 2 --output ./002.json

        Opções:
          --level N      Índice do nível (define tamanho e quantidade de setas). Padrão: 2
          --seed N       Semente RNG. Padrão: level*103+51
          --id ID        levelId no JSON (ex. 002). Padrão: D3 do level
          --output PATH  Arquivo .json; se omitido, imprime no console
          --retries N    Tentativas com seed+attempt. Padrão: 50
        """);
}

static int ParseIntArg(string[] args, string name, int defaultValue)
{
    var index = Array.IndexOf(args, name);
    if (index < 0 || index + 1 >= args.Length)
    {
        return defaultValue;
    }

    return int.TryParse(args[index + 1], out var value) ? value : defaultValue;
}

static string? ParseStringArg(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    if (index < 0 || index + 1 >= args.Length)
    {
        return null;
    }

    return args[index + 1];
}
