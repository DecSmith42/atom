﻿namespace DecSm.Atom.Args;

[PublicAPI]
public sealed record CommandLineArgs(IArg[] Args)
{
    public bool HasHelp => Args.Any(arg => arg is HelpArg);

    public bool HasGen => Args.Any(arg => arg is GenArg);

    public bool HasSkip => Args.Any(arg => arg is SkipArg);

    public bool HasHeadless => Args.Any(arg => arg is HeadlessArg);

    public bool HasVerbose => Args.Any(arg => arg is VerboseArg);

    public ParamArg[] Params =>
        Args
            .OfType<ParamArg>()
            .ToArray();

    public CommandArg[] Commands =>
        Args
            .OfType<CommandArg>()
            .ToArray();
}