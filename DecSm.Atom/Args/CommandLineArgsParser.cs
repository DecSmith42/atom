namespace DecSm.Atom.Args;

internal sealed class CommandLineArgsParser(IBuildDefinition buildDefinition, IAnsiConsole console)
{
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
                             string.Equals(argParam, buildParam.Value.Attribute.ArgName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (i == rawArgs.Count - 1)
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");

                    var nextArg = rawArgs[i + 1];

                    if (nextArg.StartsWith("--"))
                        throw new ArgumentException($"Missing value for parameter '{argParam}'");

                    args.Add(new ParamArg(buildParam.Value.Attribute.ArgName, buildParam.Key, nextArg));
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
                .Select(paramDefinition => paramDefinition.Attribute.ArgName)
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
