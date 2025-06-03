namespace DecSm.Atom.Args;

/// <summary>
///     Parses command-line arguments for an Atom build process.
///     It identifies known options (like help, generate, skip, headless, verbose, project),
///     parameters (defined in the build definition), and commands (targets to be executed).
/// </summary>
/// <remarks>
///     <para>
///         This parser is a core component of the Atom framework's command-line interface.
///         It takes raw string arguments and converts them into a structured <see cref="CommandLineArgs" /> object.
///         The parser relies on an <see cref="IBuildDefinition" /> to understand available targets and their parameters.
///         It also uses an <see cref="Spectre.Console.IAnsiConsole" /> for displaying error messages or help information related to argument
///         parsing.
///     </para>
///     <para>
///         Ambiguous or unknown arguments will result in the <see cref="CommandLineArgs.IsValid" /> property being set to false,
///         and diagnostic messages may be printed to the console.
///     </para>
/// </remarks>
internal sealed class CommandLineArgsParser(IBuildDefinition buildDefinition, IAnsiConsole console)
{
    /// <summary>
    ///     Parses the provided raw command-line arguments into a structured <see cref="CommandLineArgs" /> object.
    /// </summary>
    /// <param name="rawArgs">
    ///     A read-only list of string arguments, typically obtained from the application's entry point (e.g., `string[] args` in
    ///     `Main`).
    /// </param>
    /// <returns>
    ///     A <see cref="CommandLineArgs" /> object representing the parsed arguments.
    ///     The <see cref="CommandLineArgs.IsValid" /> property will indicate if parsing was successful and all arguments were recognized.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if a parameter requiring a value is provided without one (e.g., `--project` without a project name following it, or a custom
    ///     parameter `--paramName` without its value).
    /// </exception>
    /// <example>
    ///     Given the command: <c>atom BuildProject --version 1.0.0 -s --headless</c>
    ///     The parser might produce a <see cref="CommandLineArgs" /> object containing:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><see cref="HelpArg" /> if `-h` or `--help` is provided.</description>
    ///         </item>
    ///         <item>
    ///             <description><see cref="GenArg" /> if `-g` or `--gen` is provided.</description>
    ///         </item>
    ///         <item>
    ///             <description><see cref="SkipArg" /> if `-s` or `--skip` is provided.</description>
    ///         </item>
    ///         <item>
    ///             <description><see cref="HeadlessArg" /> if `-hl` or `--headless` is provided.</description>
    ///         </item>
    ///     </list>
    /// </example>
    public CommandLineArgs Parse(IReadOnlyList<string> rawArgs)
    {
        List<IArg> args = [];

        var isValid = true;

        for (var i = 0; i < rawArgs.Count; i++)
        {
            var rawArg = rawArgs[i];

            if (TryParseOption(rawArg) is { } optionArg)
            {
                if (optionArg is ProjectArg)
                {
                    if (i == rawArgs.Count - 1)
                        throw new ArgumentException("Missing value for -[-p]roject option");

                    optionArg = new ProjectArg(rawArgs[i + 1]);
                    i++;
                }

                args.Add(optionArg);

                continue;
            }

            // Params
            if (rawArg.StartsWith("--"))
            {
                var argParam = rawArg[2..];
                var matchedParam = false;

                foreach (var buildParam in buildDefinition.ParamDefinitions.Where(buildParam =>
                             string.Equals(argParam, buildParam.Value.ArgName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (i == rawArgs.Count - 1)
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");

                    var nextArg = rawArgs[i + 1];

                    if (nextArg.StartsWith("--"))
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");

                    args.Add(new ParamArg(buildParam.Value.ArgName, buildParam.Key, nextArg));
                    i++;
                    matchedParam = true;

                    break;
                }

                if (matchedParam)
                    continue;
            }

            if (TryParseCommand(buildDefinition.TargetDefinitions, rawArg) is { } commandArg)
            {
                args.Add(commandArg);

                continue;
            }

            console.WriteLine();
            console.WriteLine($"Unknown argument '{rawArg}'", new(Color.Red));

            var commandMatches = buildDefinition
                .TargetDefinitions
                .Keys
                .OrderBy(targetDefinition => targetDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            if (commandMatches.Count > 0)
            {
                console.WriteLine();
                console.WriteLine("Similar commands", new(Color.Yellow));

                foreach (var possibleMatch in commandMatches)
                    console.WriteLine($"  {possibleMatch}");
            }

            var paramMatches = buildDefinition
                .ParamDefinitions
                .Values
                .Select(paramDefinition => paramDefinition.ArgName)
                .OrderBy(paramDefinition => paramDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            if (paramMatches.Count > 0)
            {
                console.WriteLine();
                console.WriteLine("Similar parameters", new(Color.Yellow));

                foreach (var possibleMatch in paramMatches)
                    console.WriteLine($"  --{possibleMatch}");
            }

            console.WriteLine();

            isValid = false;
        }

        return new(isValid, args.ToArray());
    }

    private static IArg? TryParseOption(string rawArg) =>
        rawArg.ToLower() switch
        {
            "-h" or "--help" => new HelpArg(),
            "-g" or "--gen" => new GenArg(),
            "-s" or "--skip" => new SkipArg(),
            "-hl" or "--headless" => new HeadlessArg(),
            "-v" or "--verbose" => new VerboseArg(),
            "-p" or "--project" => new ProjectArg(string.Empty),
            "-i" or "--interactive" => new InteractiveArg(),
            _ => null,
        };

    private static CommandArg? TryParseCommand(IReadOnlyDictionary<string, Target> targetDefinitions, string rawArg) =>
        targetDefinitions
            .Where(buildTarget => string.Equals(rawArg, buildTarget.Key, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .FirstOrDefault() is { } matchedBuildTarget
            ? new CommandArg(matchedBuildTarget)
            : null;
}
