namespace DecSm.Atom.Args;

/// <summary>
///     Interface for a parsed command line argument.
/// </summary>
[PublicAPI]
public interface IArg;

/// <summary>
///     A command (planned execution of a target) argument.
///     Must match a target name in the build.
/// </summary>
/// <param name="Name">The name of the target to execute.</param>
[PublicAPI]
public sealed record CommandArg(string Name) : IArg;

/// <summary>
///     Instructs the build to generate/regenerate all workflow files.
/// </summary>
[PublicAPI]
public sealed record GenArg : IArg;

/// <summary>
///     Requests help information for the build.
/// </summary>
[PublicAPI]
public sealed record HelpArg : IArg;

/// <summary>
///     Represents a parameter argument with a name and value.
/// </summary>
/// <param name="ArgName">The argument name as specified in the command line.</param>
/// <param name="ParamName">The parameter name as defined in the build definition.</param>
/// <param name="ParamValue">The value of the parameter.</param>
[PublicAPI]
public sealed record ParamArg(string ArgName, string ParamName, string ParamValue) : IArg;

/// <summary>
///     Instructs the build to skip dependent targets.
/// </summary>
[PublicAPI]
public sealed record SkipArg : IArg;

/// <summary>
///     Instructs the build to run in headless mode.
///     Typically used to run the build on a CI server.
/// </summary>
[PublicAPI]
public sealed record HeadlessArg : IArg;

/// <summary>
///     Instructs the build to run in verbose mode.
///     Typically used to get more detailed output for debugging or validation.
/// </summary>
[PublicAPI]
public sealed record VerboseArg : IArg;

/// <summary>
///     Specifies the project name for the build.
/// </summary>
/// <remarks>
///     If not specified, the build will use the default project name '_atom'
/// </remarks>
/// <param name="ProjectName">The name of the project.</param>
[PublicAPI]
public sealed record ProjectArg(string ProjectName) : IArg;
