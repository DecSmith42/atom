namespace DecSm.Atom.Tool.Commands;

internal static class RunHandler
{
    public static async Task<int> Handle(RunArgs runArgs, ProjectOption projectOption, CancellationToken cancellationToken)
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory?.Exists is true)
        {
            if (Directory.Exists(Path.Combine(currentDirectory.FullName, "_atom")))
            {
                // Sanitize arguments
                var escapedArgs = runArgs.Args.Select(arg =>
                {
                    arg = arg
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);

                    return arg.Contains(';') || arg.Contains('&') || arg.Contains('|') || arg.Contains(' ')
                        ? $"\"{arg}\""
                        : arg;
                });

                var atomProjectName = "_atom";

                for (var i = 0; i < runArgs.Args.Length; i++)
                    if (projectOption.Project is { Length: > 0 })
                    {
                        atomProjectName = projectOption
                            .Project
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty);

                        if (atomProjectName.Contains(';') ||
                            atomProjectName.Contains('&') ||
                            atomProjectName.Contains('|') ||
                            atomProjectName.Contains(' '))
                            atomProjectName = $"\"{atomProjectName}\"";

                        break;
                    }

                var atomProjectPath = Path.Combine(currentDirectory.FullName, atomProjectName, $"{atomProjectName}.csproj");
                var allArgs = new[] { "run", "--project", atomProjectPath, "--" }.Concat(escapedArgs);

                var atomProcess = Process.Start("dotnet", allArgs);
                await atomProcess.WaitForExitAsync(cancellationToken);

                return atomProcess.ExitCode;
            }

            currentDirectory = currentDirectory.Parent;
        }

        Console.WriteLine("No Atom project found.");

        return 1;
    }
}
