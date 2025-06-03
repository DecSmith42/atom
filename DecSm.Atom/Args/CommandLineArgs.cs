namespace DecSm.Atom.Args;

/// <summary>
///     Contains the parsed command-line arguments for an Atom build execution.
///     This record is central to the Atom framework, providing a structured representation of user input
///     and enabling various components like <see cref="DecSm.Atom.AtomService" /> to determine the build's course of action.
/// </summary>
/// <remarks>
///     An instance of <c>CommandLineArgs</c> is typically created by the <see cref="CommandLineArgsParser" />
///     after parsing the raw string arguments provided to the Atom application.
///     It includes boolean flags for common arguments (e.g., help, generate, verbose) and collections
///     for specific command and parameter arguments.
/// </remarks>
/// <param name="Valid">
///     A boolean indicating whether the command-line arguments were parsed successfully and are valid.
///     If <c>false</c>, the Atom application will typically terminate or display help, as the build cannot proceed reliably.
/// </param>
/// <param name="Args">
///     A read-only list of <see cref="IArg" /> objects representing the individual arguments parsed from the command line.
///     This list provides access to the raw argument objects, such as <see cref="HelpArg" />, <see cref="CommandArg" />,
///     or <see cref="ParamArg" />.
/// </param>
[PublicAPI]
public sealed record CommandLineArgs(bool Valid, IReadOnlyList<IArg> Args)
{
    /// <summary>
    ///     Gets a value indicating whether the command-line arguments were parsed without error and are considered valid.
    ///     If the arguments are invalid (e.g., unrecognized commands or malformed parameters), the build process
    ///     should ideally not proceed to execution.
    /// </summary>
    /// <value><c>true</c> if the arguments are valid; otherwise, <c>false</c>.</value>
    /// <example>
    ///     If a user runs `atom --unknown-option`, <c>IsValid</c> would likely be <c>false</c>.
    ///     <code>
    /// if (!args.IsValid)
    /// {
    ///     Environment.ExitCode = 1;
    ///     return; // AtomService exits if args are invalid
    /// }
    /// </code>
    /// </example>
    public bool IsValid => Valid;

    /// <summary>
    ///     Gets a value indicating whether the help argument (<c>-h</c> or <c>--help</c>) was provided.
    ///     When <c>true</c>, the Atom application should display help information and typically exit.
    /// </summary>
    /// <value><c>true</c> if the help argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom --help` or `atom -h` will result in <c>HasHelp</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="HelpArg" />
    /// <seealso cref="DecSm.Atom.Help.IHelpService" />
    public bool HasHelp => Args.Any(arg => arg is HelpArg);

    /// <summary>
    ///     Gets a value indicating whether the generate argument (<c>-g</c> or <c>--gen</c>) was provided.
    ///     This flag instructs the Atom framework to (re)generate workflow files (e.g., for CI/CD systems).
    /// </summary>
    /// <value><c>true</c> if the generate argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom --gen` or `atom -g` will result in <c>HasGen</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="GenArg" />
    /// <seealso cref="DecSm.Atom.Workflows.WorkflowGenerator" />
    public bool HasGen => Args.Any(arg => arg is GenArg);

    /// <summary>
    ///     Gets a value indicating whether the skip argument (<c>-s</c> or <c>--skip</c>) was provided.
    ///     This flag typically instructs the build executor to skip the execution of dependent targets.
    /// </summary>
    /// <value><c>true</c> if the skip argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom MyTarget --skip` or `atom MyTarget -s` will result in <c>HasSkip</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="SkipArg" />
    /// <seealso cref="DecSm.Atom.Build.BuildExecutor" />
    public bool HasSkip => Args.Any(arg => arg is SkipArg);

    /// <summary>
    ///     Gets a value indicating whether the headless argument (<c>-hl</c> or <c>--headless</c>) was provided.
    ///     This mode is often used for non-interactive environments like CI/CD pipelines, where user prompts
    ///     or interactive features should be disabled.
    /// </summary>
    /// <value><c>true</c> if the headless argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom MyCommand --headless` or `atom MyCommand -hl` will result in <c>HasHeadless</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="HeadlessArg" />
    public bool HasHeadless => Args.Any(arg => arg is HeadlessArg);

    /// <summary>
    ///     Gets a value indicating whether the verbose argument (<c>-v</c> or <c>--verbose</c>) was provided.
    ///     When <c>true</c>, the Atom application should output more detailed logging information, which can be
    ///     useful for debugging or diagnostics.
    /// </summary>
    /// <value><c>true</c> if the verbose argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom MyCommand --verbose` or `atom MyCommand -v` will result in <c>HasVerbose</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="VerboseArg" />
    /// <seealso cref="DecSm.Atom.Logging.LogOptions.IsVerboseEnabled" />
    public bool HasVerbose => Args.Any(arg => arg is VerboseArg);

    /// <summary>
    ///     Gets a value indicating whether the project argument (<c>-p</c> or <c>--project</c>) was provided.
    ///     This argument allows specifying a custom Atom project directory/name, overriding the default.
    /// </summary>
    /// <value><c>true</c> if the project argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom -p MyAtomProject` will result in <c>HasProject</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="ProjectArg" />
    /// <seealso cref="ProjectName" />
    public bool HasProject => Args.Any(arg => arg is ProjectArg);

    /// <summary>
    ///     Gets a value indicating whether the interactive argument (<c>-i</c> or <c>--interactive</c>) was provided.
    ///     This flag suggests that the Atom application may prompt the user for input if needed for certain parameters.
    /// </summary>
    /// <value><c>true</c> if the interactive argument is present; otherwise, <c>false</c>.</value>
    /// <example>
    ///     Running `atom MyCommand -i` will result in <c>HasInteractive</c> being <c>true</c>.
    /// </example>
    /// <seealso cref="InteractiveArg" />
    /// <seealso cref="DecSm.Atom.Params.ParamService" />
    public bool HasInteractive => Args.Any(arg => arg is InteractiveArg);

    /// <summary>
    ///     Gets a read-only list of parameter arguments (<see cref="ParamArg" />) provided on the command line.
    ///     Each <see cref="ParamArg" /> typically represents a key-value pair (e.g., <c>--my-param value</c>).
    /// </summary>
    /// <value>A list of <see cref="ParamArg" /> objects.</value>
    /// <example>
    ///     For `atom --param1 value1 --param2 value2`, <c>Params</c> would contain two <see cref="ParamArg" /> objects.
    /// </example>
    /// <seealso cref="ParamArg" />
    public IReadOnlyList<ParamArg> Params =>
        Args
            .OfType<ParamArg>()
            .ToList();

    /// <summary>
    ///     Gets a read-only list of command arguments (<see cref="CommandArg" />) provided on the command line.
    ///     Each <see cref="CommandArg" /> represents a target name that the user intends to execute.
    /// </summary>
    /// <value>A list of <see cref="CommandArg" /> objects.</value>
    /// <example>
    ///     For `atom Clean Build Test`, <c>Commands</c> would contain three <see cref="CommandArg" /> objects: "Clean", "Build", and "Test".
    /// </example>
    /// <seealso cref="CommandArg" />
    public IReadOnlyList<CommandArg> Commands =>
        Args
            .OfType<CommandArg>()
            .ToList();

    /// <summary>
    ///     Gets the project name specified by the project argument (<c>-p</c> or <c>--project</c>).
    ///     If the project argument was not provided, this defaults to "_atom".
    ///     The Atom project typically resides in a directory with this name, containing the build scripts.
    /// </summary>
    /// <value>The specified project name, or "_atom" if not specified.</value>
    /// <example>
    ///     If `atom -p MyBuildScripts` is run, <c>ProjectName</c> will be "MyBuildScripts".
    ///     If `atom Build` is run, <c>ProjectName</c> will be "_atom".
    /// </example>
    /// <seealso cref="ProjectArg" />
    /// <seealso cref="HasProject" />
    public string ProjectName =>
        Args
            .OfType<ProjectArg>()
            .Select(arg => arg.ProjectName)
            .FirstOrDefault() ??
        "_atom";
}
