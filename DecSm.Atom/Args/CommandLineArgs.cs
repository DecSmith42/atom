namespace DecSm.Atom.Args;

public sealed record CommandLineArgs(bool Valid, IReadOnlyList<IArg> Args)
{
    public bool IsValid => Valid;

    public bool HasHelp => Args.Any(arg => arg is HelpArg);

    public bool HasGen => Args.Any(arg => arg is GenArg);

    public bool HasSkip => Args.Any(arg => arg is SkipArg);

    public bool HasHeadless => Args.Any(arg => arg is HeadlessArg);

    public bool HasVerbose => Args.Any(arg => arg is VerboseArg);

    public bool HasProject => Args.Any(arg => arg is ProjectArg);

    public IReadOnlyList<ParamArg> Params =>
        Args
            .OfType<ParamArg>()
            .ToList();

    public IReadOnlyList<CommandArg> Commands =>
        Args
            .OfType<CommandArg>()
            .ToList();

    public string ProjectName =>
        Args
            .OfType<ProjectArg>()
            .Select(arg => arg.ProjectName)
            .FirstOrDefault() ??
        "_atom";
}
