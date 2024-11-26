namespace DecSm.Atom.Args;

/// <summary>
///     Contains the parsed command line arguments for the build.
/// </summary>
/// <param name="Valid">Whether the arguments were parsed without error.</param>
/// <param name="Args">The parsed arguments.</param>
[PublicAPI]
public sealed record CommandLineArgs(bool Valid, IReadOnlyList<IArg> Args)
{
    /// <summary>
    ///     Whether the arguments were parsed without error.
    ///     If the args are invalid, the build should not proceed.
    /// </summary>
    public bool IsValid => Valid;

    /// <summary>
    ///     Whether the -h / --help argument was provided.
    ///     <see cref="HelpArg" />
    /// </summary>
    public bool HasHelp => Args.Any(arg => arg is HelpArg);

    /// <summary>
    ///     Whether the -g / --gen argument was provided.
    ///     <see cref="GenArg" />
    /// </summary>
    public bool HasGen => Args.Any(arg => arg is GenArg);

    /// <summary>
    ///     Whether the -s / --skip argument was provided.
    ///     <see cref="SkipArg" />
    /// </summary>
    public bool HasSkip => Args.Any(arg => arg is SkipArg);

    /// <summary>
    ///     Whether the -hl / --headless argument was provided.
    ///     <see cref="HeadlessArg" />
    /// </summary>
    public bool HasHeadless => Args.Any(arg => arg is HeadlessArg);

    /// <summary>
    ///     Whether the -v / --verbose argument was provided.
    ///     <see cref="VerboseArg" />
    /// </summary>
    public bool HasVerbose => Args.Any(arg => arg is VerboseArg);

    /// <summary>
    ///     Whether the -p / --project argument was provided.
    ///     <see cref="ProjectArg" />
    /// </summary>
    public bool HasProject => Args.Any(arg => arg is ProjectArg);

    /// <summary>
    ///     The list of parameter arguments provided.
    ///     <see cref="ParamArg" />
    /// </summary>
    public IReadOnlyList<ParamArg> Params =>
        Args
            .OfType<ParamArg>()
            .ToList();

    /// <summary>
    ///     The list of command arguments provided.
    ///     <see cref="CommandArg" />
    /// </summary>
    public IReadOnlyList<CommandArg> Commands =>
        Args
            .OfType<CommandArg>()
            .ToList();

    /// <summary>
    ///     The project name specified by the -p / --project argument.
    ///     Defaults to "_atom" if not provided.
    ///     <see cref="ProjectArg" />
    /// </summary>
    public string ProjectName =>
        Args
            .OfType<ProjectArg>()
            .Select(arg => arg.ProjectName)
            .FirstOrDefault() ??
        "_atom";
}
