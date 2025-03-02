namespace JrCommand;

using CommandLine;

public class Options
{
    [Option("flavor", Required = false, HelpText = "Run an LF flavor")]
    public string? Flavor { get; set; }

    [Option("ldl-style", Required = false, HelpText = "LDL Style: Nothing, DateTime, LocalCrawl, AlternateCrawl")]
    public string? LDLStyle { get; set; }

    [Option("ldl-bg", Required = false, HelpText = "Enable LDL Background: 1 or 0")]
    public string? LDLBackground { get; set; }

    [Option("warning", Required = false, HelpText = "Send a weather warning with this text")]
    public string? Warning { get; set; }

    [Option("beep", Required = false, HelpText = "Trigger the wx warning relay for 1 second")]
    public bool Beep { get; set; }

    [Option("load-config", Required = false, HelpText = "Load a new config file")]
    public string? LoadConfig { get; set; }

    [Option("reload-data", Required = false, HelpText = "Reload all data")]
    public bool ReloadData { get; set; }
}