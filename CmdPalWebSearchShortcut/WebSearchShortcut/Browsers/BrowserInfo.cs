namespace WebSearchShortcut.Browsers;

public class BrowserInfo
{
  public string Name { get; set; } = string.Empty;
  public string Path { get; set; } = string.Empty;
  public string ArgumentsPattern { get; set; } = string.Empty;

  public override string ToString() => $"{Name} - {Path} {ArgumentsPattern}";
}
