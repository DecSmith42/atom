namespace Atom;

public sealed record Command(
    string Name,
    string? Description,
    List<Argument> Arguments,
    List<Option> Options,
    List<Command> SubCommands
);

public sealed record Argument(string Name, string? Description, string ValueType);

public sealed record Option(string Name, string? Description, string ValueType);

public static class DotnetCliParser
{
    public static Command? GetCommand(string name, JsonElement element)
    {
        if (element.ValueKind is not JsonValueKind.Object)
            return null;

        var description = element.TryGetProperty("description", out var descElement)
            ? descElement.GetString()
            : null;

        var arguments = new List<Argument>();

        if (element.TryGetProperty("arguments", out var argsElement))
            arguments.AddRange(argsElement
                .EnumerateObject()
                .Select(static arg =>
                {
                    var argName = arg.Name;
                    var argValue = arg.Value;

                    if (argValue.ValueKind is not JsonValueKind.Object)
                        return null;

                    var argDescription = argValue.TryGetProperty("description", out var desc)
                        ? desc.GetString()
                        : null;

                    var argValueType = argValue.TryGetProperty("valueType", out var valType)
                        ? valType.GetString()
                        : null;

                    if (argValueType is null)
                        return argName is "arguments"
                            ? new(argName, argDescription, "System.String[]")
                            : null;

                    return new Argument(argName, argDescription, argValueType);
                })
                .OfType<Argument>());

        var options = new List<Option>();

        if (element.TryGetProperty("options", out var optsElement))
            options.AddRange(optsElement
                .EnumerateObject()
                .Select(static opt =>
                {
                    var optName = opt.Name;
                    var optValue = opt.Value;

                    if (optValue.ValueKind is not JsonValueKind.Object)
                        return null;

                    var optDescription = optValue.TryGetProperty("description", out var desc)
                        ? desc.GetString()
                        : null;

                    var optValueType = optValue.TryGetProperty("valueType", out var valType)
                        ? valType.GetString()
                        : null;

                    return optValueType is null
                        ? null
                        : new Option(optName, optDescription, optValueType);
                })
                .OfType<Option>());

        var subCommands = new List<Command>();

        if (element.TryGetProperty("subcommands", out var subCmdsElement))
            subCommands.AddRange(subCmdsElement
                .EnumerateObject()
                .Select(static subCmd => GetCommand(subCmd.Name, subCmd.Value))
                .OfType<Command>());

        // If there is a framework option and not a project or solution argument, add one
        if (options.Any(static opt => opt.Name is "--framework") &&
            options.Any(static opt => opt.Name is "--configuration") &&
            !arguments.Any(static arg => arg.Name is "PROJECT | SOLUTION | FILE"))
            arguments.Insert(0,
                new("PROJECT | SOLUTION | FILE", "The project, solution, or file to operate on.", "System.String[]"));

        return new(name, description, arguments, options, subCommands);
    }

    public static void FlattenCommands(string prefix, Command subCommand, List<Command> flattenedCommands)
    {
        var prefixedName = prefix.Length > 0
            ? $"{prefix} {subCommand.Name}"
            : subCommand.Name;

        flattenedCommands.Add(subCommand with
        {
            Name = $"{prefixedName}",
        });

        foreach (var subSubCommand in subCommand.SubCommands)
            FlattenCommands($"{prefixedName}", subSubCommand, flattenedCommands);
    }
}
