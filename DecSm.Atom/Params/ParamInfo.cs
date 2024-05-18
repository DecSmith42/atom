namespace DecSm.Atom.Params;

public sealed record ParamInfo(string Name, string Description, string? DefaultValue)
{
    public required string ArgName { get; init; }
    public required bool SourceFromCliArguments { get; set; }
    public required bool SourceFromEnvironmentVariables { get; set; }
    public required bool SourceFromConfigurationFiles { get; set; }
    public required bool SourceFromSecrets { get; set; }
}