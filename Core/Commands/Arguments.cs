namespace AdventOfCode.Core.Commands;

using System.CommandLine;

static class Arguments
{
  private const int FirstAocYear = 2015;
  private const int MinDay = 1;
  private const int MaxDay = 25;

  public static Argument<int> Year()
  {
    var currentYear = DateTime.Now.Year;

    var arg = new Argument<int>("year")
    {
      Description = $"The Advent of Code year ({FirstAocYear}-{currentYear})"
    };

    arg.Validators.Add(result =>
    {
      var year = result.GetValueOrDefault<int>();
      if (year < FirstAocYear || year > currentYear + 1)
      {
        result.AddError($"Year must be between {FirstAocYear} and {currentYear + 1}.");
      }
    });

    return arg;
  }

  public static Argument<int?> Day()
  {
    var arg = new Argument<int?>("day")
    {
      Description = $"The Advent of Code day ({MinDay}-{MaxDay}, optional)",
      DefaultValueFactory = parseResult => null,
    };

    arg.Validators.Add(result =>
    {
      if (result.GetValueOrDefault<int?>() is int day)
      {
        if (day < MinDay || day > MaxDay)
        {
          result.AddError($"Day must be between {MinDay} and {MaxDay}");
        }
      }
    });

    return arg;
  }

  public static Option<bool> UseTestInput()
  {
    var option = new Option<bool>("--test", ["-t"])
    {
      Description = "Use test input instead of actual input",
      DefaultValueFactory = _ => false
    };

    return option;
  }

  public static Option<int?> SelectedPart()
  {
    var option = new Option<int?>("--part", ["-p"])
    {
      Description = "Run only specific part (1 or 2)"
    };

    option.Validators.Add(result =>
      {
        if (result.GetValueOrDefault<int?>() is int part && (part < 1 || part > 2))
        {
          result.AddError("Part must be 1 or 2.");
        }
      });

    return option;
  }
}
