namespace Atom;

public static class DotnetCliGenerator
{
    public static void GenerateInterfaceCode(CsharpWriter writer, Command command)
    {
        using (writer.Block("public partial interface IDotnetCli"))
        {
            GenerateCommandMethod(writer, command, true);

            if (command.Arguments.Count is 0)
                return;

            var singleArg = command.Arguments[0];

            // Generate overload for single argument
            if (singleArg.ValueType.EndsWith("[]", StringComparison.Ordinal))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = singleArg.ValueType.Replace("[]", string.Empty),
                            },
                        ],
                    },
                    true);

            // Generate overload for a single argument that is a path
            if (singleArg.ValueType is "System.String" or "System.String[]" &&
                (singleArg.Name.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 singleArg.Name.Contains("file", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("file", StringComparison.OrdinalIgnoreCase)))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = "DecSm.Atom.Paths.RootedPath",
                            },
                        ],
                    },
                    true);
        }

        writer.WriteLine(string.Empty);
    }

    public static void GenerateImplementationCode(CsharpWriter writer, Command command)
    {
        using (writer.Block("internal partial class DotnetCli : IDotnetCli"))
        {
            GenerateCommandMethod(writer, command, false);

            if (command.Arguments.Count is 0)
                return;

            var singleArg = command.Arguments[0];

            // Generate overload for single argument
            if (singleArg.ValueType.EndsWith("[]", StringComparison.Ordinal))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = singleArg.ValueType.Replace("[]", string.Empty),
                            },
                        ],
                    },
                    false);

            // Generate overload for a single argument that is a path
            if (singleArg.ValueType is "System.String" or "System.String[]" &&
                (singleArg.Name.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 singleArg.Name.Contains("file", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("file", StringComparison.OrdinalIgnoreCase)))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = "DecSm.Atom.Paths.RootedPath",
                            },
                        ],
                    },
                    false);
        }

        writer.WriteLine(string.Empty);
    }

    public static void GenerateOptionsCode(CsharpWriter writer, Command command)
    {
        if (command.Options.Count is 0)
            return;

        writer.WriteLine("[PublicAPI]");

        using (writer.Block($"public sealed record {CsharpWriter.ToPascalCase(command.Name)}Options"))
        {
            foreach (var option in command.Options.Where(x => x.ValueType is not (null or "System.Void")))
                writer.WriteProperty("public",
                    Type.GetType(option.ValueType) is not null
                        ? $"{option.ValueType}?"
                        : "string?",
                    CsharpWriter.ToPascalCase(option.Name),
                    new(null),
                    new(null, true),
                    null,
                    option.Description);

            writer.WriteMethod("public override",
                "string",
                "ToString",
                Array.Empty<MethodParam>(),
                bodyWriter =>
                {
                    bodyWriter.WriteLine("return string.Join(' ',");

                    using (bodyWriter.Block("new[]"))
                    {
                        foreach (var option in command.Options.Where(x => x.ValueType is not (null or "System.Void")))
                        {
                            bodyWriter.WriteLine(option.ValueType is "System.Boolean"
                                ? $"{CsharpWriter.ToPascalCase(option.Name)} is true"
                                : $"{CsharpWriter.ToPascalCase(option.Name)} is not null");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine(option.ValueType is "System.Boolean"
                                    ? $"? \"{option.Name}\""
                                    : $"? $\"{option.Name} {{{CsharpWriter.ToPascalCase(option.Name)}}}\"");

                                bodyWriter.WriteLine(": null,");
                            }
                        }
                    }

                    bodyWriter.WriteLine(");");
                });
        }

        writer.WriteLine(string.Empty);
    }

    private static void GenerateCommandMethod(CsharpWriter writer, Command command, bool isStub) =>
        writer.WriteMethod("public",
            "Task<ProcessRunResult>",
            CsharpWriter.ToPascalCase(command.Name),
            command
                .Arguments
                .Select(arg => new MethodParam(Type.GetType(arg.ValueType) is null
                        ? arg.ValueType is "DecSm.Atom.Paths.RootedPath"
                            ? "DecSm.Atom.Paths.RootedPath"
                            : "System.String"
                        : arg.ValueType,
                    CsharpWriter.ToPascalCase(arg.Name, true),
                    XmlDescription: arg.Description))
                .Append(command.Options.Count > 0
                    ? new MethodParam($"{CsharpWriter.ToPascalCase(command.Name)}Options?",
                        "options",
                        "null",
                        "Options")
                    : null)
                .Append(new("CancellationToken", "cancellationToken", "default", "Cancellation token"))
                .ToArray(),
            isStub
                ? null
                : bodyWriter =>
                {
                    switch (command)
                    {
                        case { Arguments.Count: 0, Options.Count: 0 }:
                            bodyWriter.WriteLine(
                                "processRunner.RunAsync(new(\"dotnet\", string.Empty), cancellationToken);");

                            return;
                        case { Arguments.Count: 0, Options.Count: > 0 }:
                            bodyWriter.WriteLine(
                                "processRunner.RunAsync(new(\"dotnet\", (options ?? new()).ToString()), cancellationToken);");

                            return;
                        default:
                            bodyWriter.WriteLine("processRunner.RunAsync(new(\"dotnet\",");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine("string.Join(' ', [");

                                using (bodyWriter.Indent)
                                {
                                    bodyWriter.WriteLine($"\"{command.Name}\",");

                                    foreach (var arg in command.Arguments)
                                        bodyWriter.WriteLine(arg.ValueType.EndsWith("[]", StringComparison.Ordinal)
                                            ? $"string.Join(' ', {CsharpWriter.ToPascalCase(arg.Name, true)}),"
                                            : $"{CsharpWriter.ToPascalCase(arg.Name, true)},");

                                    if (command.Options.Count > 0)
                                        bodyWriter.WriteLine("options?.ToString(),");
                                }

                                bodyWriter.WriteLine("])), cancellationToken);");
                            }

                            return;
                    }
                },
            true,
            isStub
                ? command.Description
                : null);
}
