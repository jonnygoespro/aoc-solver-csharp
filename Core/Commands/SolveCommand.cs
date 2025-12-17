namespace AdventOfCode.Core.Commands;

using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

internal static class SolveCommand
{
  private const string Name = "solve";
  private const string Description = "Run solutions for the specified year/day";

  public static Command Create()
  {
    var yearArg = Arguments.Year();
    var dayArg = Arguments.Day();
    var testOption = Arguments.UseTestInput();
    var selectedPartOption = Arguments.SelectedPart();

    Command command = new(Name, Description)
    {
     yearArg,
     dayArg,
     testOption,
     selectedPartOption
    };

    command.SetAction(async (parseResult) =>
    {
      int year = parseResult.GetValue(yearArg);
      int? day = parseResult.GetValue(dayArg);
      bool useTest = parseResult.GetValue(testOption);
      int? part = parseResult.GetValue(selectedPartOption);
      await Execute(year, day, useTest, part);
    });

    return command;
  }

  private static async Task Execute(int year, int? day, bool useTest, int? part)
  {
    try
    {
      if (day.HasValue)
      {
        await SolveDayAsync(year, day.Value, useTest, part);
      }
      else
      {
        await SolveAllDaysAsync(year, useTest, part);
      }
    }
    catch (Exception e)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"Error: {e.Message}");
      Console.ResetColor();
      Environment.Exit(1);
    }
  }

  private static async Task SolveDayAsync(int year, int day, bool useTest, int? part)
  {
    var solver = GetSolver(year, day);

    Console.WriteLine($"╔═══════════════════════════════════════╗");
    Console.WriteLine($"║  Advent of Code {year} - Day {day:D2}         ║");
    Console.WriteLine($"╚═══════════════════════════════════════╝");

    if (!part.HasValue || part == 1)
    {
      var inputType = useTest ? "TestInputPart1.txt" : "Input.txt";
      var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
      solver.SetInputFilePath(inputPath);
      solver.Setup();
      Console.WriteLine($"Input: {inputType}");
      await RunPartAsync(1, solver.Part1);
    }

    if (!part.HasValue || part == 2)
    {
      var inputType = useTest ? "TestInputPart1.txt" : "Input.txt";
      var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
      solver.SetInputFilePath(inputPath);
      solver.Setup();
      Console.WriteLine($"\nInput: {inputType}");
      await RunPartAsync(2, solver.Part2);
    }
  }

  private static async Task RunPartAsync(int partNumber, Func<ValueTask<string>> partFunc)
  {
    Console.Write($"Part {partNumber}: ");

    var sw = Stopwatch.StartNew();
    var result = await partFunc();
    sw.Stop();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(result);
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($" ({sw.ElapsedMilliseconds}ms)");
    Console.ResetColor();
  }

  private static async Task SolveAllDaysAsync(int year, bool useTest, int? part)
  {
    var solvers = GetAllSolvers(year);
    
    if (solvers.Count == 0)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"No solutions found for year {year}");
      Console.ResetColor();
      return;
    }

    Console.WriteLine($"╔═══════════════════════════════════════╗");
    Console.WriteLine($"║  Advent of Code {year} - All Days       ║");
    Console.WriteLine($"╚═══════════════════════════════════════╝\n");

    foreach (var (day, solverType) in solvers)
    {
      var solver = (BaseDay)Activator.CreateInstance(solverType)!;
      var inputType = useTest ? "TestInput.txt" : "Input.txt";
      var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";

      if (!File.Exists(inputPath))
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Day {day:D2}: Skipped (no input file)");
        Console.ResetColor();
        continue;
      }

      try
      {
        Console.Write($"Day {day:D2}: ");
        solver.SetInputFilePath(inputPath);
        solver.Setup();

        if (!part.HasValue || part == 1)
        {
          var result1 = await solver.Part1();
          Console.Write($"P1={result1} ");
        }

        if (!part.HasValue || part == 2)
        {
          var result2 = await solver.Part2();
          Console.Write($"P2={result2}");
        }

        Console.WriteLine();
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
      }
    }
  }

  private static BaseDay GetSolver(int year, int day)
  {
    var typeName = $"AdventOfCode.Solutions.Year{year}.Day{day:D2}";

    var solverType = Assembly.GetExecutingAssembly()
        .GetTypes()
        .FirstOrDefault(t => typeof(BaseDay).IsAssignableFrom(t) && t.FullName == typeName);

    if (solverType == null)
    {
      throw new InvalidOperationException(
        $"No solver found for Year {year}, Day {day}.\n" +
        $"Expected class: {typeName}\n" +
        $"Run 'create {year} {day}' to generate the boilerplate.");
    }

    return (BaseDay)Activator.CreateInstance(solverType)!;
  }

  private static List<(int day, Type type)> GetAllSolvers(int year)
  {
    var solvers = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(BaseDay).IsAssignableFrom(t)
            && t.Namespace == $"AdventOfCode.Solutions.Year{year}"
            && !t.IsAbstract)
        .Select(t =>
        {
          var dayStr = t.Name.Replace("Day", "");
          if (int.TryParse(dayStr, out var day))
            return (day, type: t);

          return (0, type: t);
        })
        .Where(x => x.Item1 > 0)
        .OrderBy(x => x.Item1)
        .ToList();

    return solvers;
  }
}
