namespace DecSm.Atom.Args;

public sealed record CommandLineArgs(IArg[] Args)
{
    public bool HasHelp => Args.Any(arg => arg is HelpArg);
    public bool HasGen => Args.Any(arg => arg is GenArg);
    public bool HasSkip => Args.Any(arg => arg is SkipArg);
    public ParamArg[] Params => Args.OfType<ParamArg>().ToArray();
    public CommandArg[] Commands => Args.OfType<CommandArg>().ToArray();
    public string? GetParamByName(string name) => Params.FirstOrDefault(p => p.ParamName == name)?.ParamValue;
    public bool HasCommand(string name) => Commands.Any(c => c.Name == name);
}