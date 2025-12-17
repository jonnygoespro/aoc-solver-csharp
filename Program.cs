using System.CommandLine;
using AdventOfCode.Core.Commands;

namespace AdventOfCode;

public class Program
{
  public static async Task<int> Main(string[] args)
  {
    var rootCommand = new RootCommand(
      "Advent of Code Solver - Create and solve AoC challenges"
    )
    {
      CreateCommand.Create(),
      SolveCommand.Create()
    };

    rootCommand.Description += @"

Examples:
  dotnet run -- create 2024 1              Create structure for 2024 Day 1
  dotnet run -- solve 2024 1               Solve 2024 Day 1 with test inputs
  dotnet run -- solve 2024 1 --actual      Solve 2024 Day 1 with actual input
  dotnet run -- solve 2024 1 -a            Same as above (short form)
";

    return await rootCommand.Parse(args).InvokeAsync();
  }
}

