namespace AdventOfCode.Core.Commands;

using System.CommandLine;

internal static class CreateCommand
{
  private const string Name = "create";
  private const string Description = "Initializes folders and boilerplate files for a given year/day.";
  private const string dayTemplatePath = "./Core/Templates/DayTemplate.txt";

  public static Command Create()
  {
    var yearArg = Arguments.Year();
    var dayArg = Arguments.Day();

    var command = new Command(Name, Description)
    {
     yearArg,
     dayArg
    };

    command.SetAction(async (parseResult) =>
    {
      int year = parseResult.GetValue(yearArg);
      int? day = parseResult.GetValue(dayArg);
      await Execute(year, day);
    });

    return command;
  }

  private static async Task Execute(int year, int? day)
  {
    try
    {
      if (day.HasValue)
      {
        await CreateDayStructure(year, day.Value);
        Console.WriteLine($"✓ Successfully created structure for {year} Day {day.Value:D2}");
        Console.WriteLine($"  Solution file: {year}/Day{day.Value:D2}.cs");
        Console.WriteLine($"  Input folder: {year}/inputs/Day{day.Value:D2}/");
      }
      else
      {
        await CreateYearStructure(year);
        Console.WriteLine($"✓ Successfully created structure for year {year}");
      }
    }
    catch (Exception e)
    {
      Console.Error.WriteLine($"✗ Error: {e.Message}");
    }
  }

  private static async Task CreateYearStructure(int year)
  {
    var basePath = $"./Solutions/{year}";
    Directory.CreateDirectory(basePath);
    Directory.CreateDirectory($"{basePath}/inputs");

    Console.WriteLine($"Created directory structure for year {year}");
  }

  private static async Task CreateDayStructure(int year, int day)
  {
    var basePath = $"./Solutions/{year}";
    var inputPath = $"{basePath}/inputs/Day{day:D2}";

    Directory.CreateDirectory(basePath);
    Directory.CreateDirectory(inputPath);

    await CreateEmptyFile($"{inputPath}/TestInputPart1.txt");
    await CreateEmptyFile($"{inputPath}/TestInputPart2.txt");
    await CreateEmptyFile($"{inputPath}/Input.txt");

    await CreateSolutionFile(year, day, $"{basePath}/Day{day:D2}.cs");
  }

  private static async Task CreateEmptyFile(string path)
  {
    if (!File.Exists(path))
    {
      await using var fs = File.Create(path);
    }
  }

  private static async Task CreateSolutionFile(int year, int day, string outputPath)
  {
    if (File.Exists(outputPath))
    {
      Console.WriteLine($"  Warning: {outputPath} already exists, skipping...");
      return;
    }

    var template = File.ReadAllText(dayTemplatePath);

    var content = template
      .Replace("DD", $"{day:D2}")
      .Replace("YYYY", year.ToString());

    await File.WriteAllTextAsync(outputPath, content);
  }
}
