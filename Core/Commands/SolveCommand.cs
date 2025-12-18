namespace AdventOfCode.Core.Commands;

using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using Spectre.Console;

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
      Console.Clear();

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
      AnsiConsole.MarkupLine($"[red]Error: {e.Message.EscapeMarkup()}[/]");
      Environment.Exit(1);
    }
  }

  private static async Task SolveDayAsync(int year, int day, bool useTest, int? part)
  {
    var solver = GetSolver(year, day);
    string result1 = "";
    string result2 = "";
    double elapsed1 = 0;
    double elapsed2 = 0;

    if (!part.HasValue || part == 1)
    {
      var inputType = useTest ? "TestInputPart1.txt" : "Input.txt";
      var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
      solver.SetInputFilePath(inputPath);
      solver.Setup();

      (result1, elapsed1) = await RunPartAsync(solver.Part1);
    }

    if (!part.HasValue || part == 2)
    {
      var inputType = useTest ? "TestInputPart2.txt" : "Input.txt";
      var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
      solver.SetInputFilePath(inputPath);
      solver.Setup();

      (result2, elapsed2) = await RunPartAsync(solver.Part2);
    }

    PrintDayTable(year, day, result1, elapsed1, result2, elapsed2);
  }

  private static string GetColorByElapsed(double ms)
  {
    if (ms < 50) return "darkgreen";
    if (ms < 200) return "orange1";
    return "red";
  }

  private static void PrintDayTable(int year, int day, string result1, double elapsed1, string result2, double elapsed2)
  {
    var table = new Table()
          .Title($"[grey][[ [darkgreen]🎄 Advent of Code {year} Day {day}[/] ]][/]\n")
          .Border(TableBorder.Rounded)
          .BorderColor(Color.Grey)
          .AddColumn(new TableColumn("[bold]Day[/]").LeftAligned().Width(10).Padding(2, 0))
          .AddColumn(new TableColumn("[bold]Part[/]").LeftAligned().Width(10).Padding(2, 0))
          .AddColumn(new TableColumn("[bold]Solution[/]").RightAligned().Width(20).Padding(2, 0))
          .AddColumn(new TableColumn("[bold]Elapsed Time[/]").RightAligned().Width(20).Padding(2, 0));

    table.AddRow(
            $"[grey46]Day {day}[/]",
            $"[grey46]Part 1[/]",
            $"[grey]{result1.EscapeMarkup()}[/]",
            $"[{GetColorByElapsed(elapsed1)}]{elapsed1:F3} ms[/]"
          );

    table.AddRow(
            $"[grey46]Day {day}[/]",
            $"[grey46]Part 2[/]",
            $"[grey]{result2.EscapeMarkup()}[/]",
            $"[{GetColorByElapsed(elapsed2)}]{elapsed2:F3} ms[/]"
          );

    AnsiConsole.WriteLine();
    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
  }

  private static void PrintYearTable(int year, List<(int day, int part, string result, double elapsed)> parts)
  {
    double totalElapsed = parts.Sum(p => p.elapsed);

    var table = new Table()
        .Title($"[grey][[ [darkgreen]🎄 Advent of Code {year}[/] ]][/]\n")
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey50)
        .ShowFooters()
        .AddColumn(new TableColumn("[bold]Day[/]").LeftAligned().Width(10).Padding(2, 0))
        .AddColumn(new TableColumn("[bold]Part[/]").LeftAligned().Width(10).Padding(2, 0))
        .AddColumn(new TableColumn("[bold]Solution[/]").RightAligned().Width(20).Padding(2, 0))
        .AddColumn(new TableColumn("[bold]Elapsed Time[/]").Footer($"[bold]{totalElapsed:F3} ms[/]").RightAligned().Width(20).Padding(2, 0));

    for (int i = 0; i < parts.Count; i++)
    {
      var part = parts[i];
      bool isFirstPartOfDay = i == 0 || parts[i - 1].day != part.day;
      string dayDisplay = isFirstPartOfDay ? $"[grey46]Day {part.day}[/]" : "";

      table.AddRow(
          dayDisplay,
          $"[grey46]Part {part.part}[/]",
          $"[grey]{part.result.EscapeMarkup()}[/]",
          $"[{GetColorByElapsed(part.elapsed)}]{part.elapsed:F3} ms[/]"
      );

      bool isEndOfDay = i + 1 < parts.Count && parts[i + 1].day != part.day;
      if (isEndOfDay)
      {
        table.AddEmptyRow();
      }
    }

    AnsiConsole.WriteLine();
    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
  }

  private static async Task<(string result, double elapsed)> RunPartAsync(Func<ValueTask<string>> partFunc)
  {
    var sw = Stopwatch.StartNew();
    var result = await partFunc();
    sw.Stop();
    double ms = sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
    return (result, ms);
  }

  private static async Task SolveAllDaysAsync(int year, bool useTest, int? part)
  {
    var solvers = GetAllSolvers(year);

    if (solvers.Count == 0)
    {
      AnsiConsole.MarkupLine($"[red]No solutions found for year {year}[/]");
      return;
    }

    var parts = new List<(int, int, string, double)>();

    foreach (var (day, solverType) in solvers)
    {
      var solver = (BaseDay)Activator.CreateInstance(solverType)!;

      try
      {
        if (!part.HasValue || part == 1)
        {
          var inputType = useTest ? "TestInputPart1.txt" : "Input.txt";
          var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
          if (!File.Exists(inputPath))
          {
            continue;
          }

          solver.SetInputFilePath(inputPath);
          solver.Setup();
          var (result, elapsed) = await RunPartAsync(solver.Part1);
          parts.Add((day, 1, result, elapsed));
        }

        if (!part.HasValue || part == 2)
        {
          var inputType = useTest ? "TestInputPart2.txt" : "Input.txt";
          var inputPath = $"./Solutions/{year}/Inputs/Day{day:D2}/{inputType}";
          if (!File.Exists(inputPath))
          {
            continue;
          }

          solver.SetInputFilePath(inputPath);
          solver.Setup();
          var (result, elapsed) = await RunPartAsync(solver.Part2);
          parts.Add((day, 2, result, elapsed));
        }
      }
      catch (Exception e)
      {
        AnsiConsole.MarkupLine($"[red]  Day {day:D2} Error: {e.Message.EscapeMarkup()}[/]");
      }
    }

    PrintYearTable(year, parts);
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
