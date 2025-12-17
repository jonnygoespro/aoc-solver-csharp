namespace AdventOfCode.Core;

public abstract class BaseDay
{
  private string? _inputFilePath;

  public string InputFilePath
  {
    get => _inputFilePath ?? throw new InvalidOperationException(
        "InputFilePath has not been initialized. Call Initialize() first.");
    private set => _inputFilePath = value;
  }

  internal void SetInputFilePath(string inputFilePath)
  {
    if (string.IsNullOrWhiteSpace(inputFilePath))
    {
      throw new ArgumentException("Input path cannot be null or empty.", nameof(inputFilePath));
    }

    if (!File.Exists(inputFilePath))
    {
      throw new FileNotFoundException($"Input file not found: {inputFilePath}", inputFilePath);
    }

    InputFilePath = inputFilePath;
  }

  public abstract void Setup();

  public virtual ValueTask<string> Part1()
  {
    return new ValueTask<string>("0");
  }

  public virtual ValueTask<string> Part2()
  {
    return new ValueTask<string>("0");
  }

}
