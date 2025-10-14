namespace DecSm.Atom.Tool.Commands;

internal static class RunCommand
{
    public static async Task<int> Handle(string[] runArgs, string project, CancellationToken cancellationToken)
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory?.Exists is true)
        {
            if (Directory.Exists(Path.Combine(currentDirectory.FullName, project)))
            {
                // Sanitize arguments
                var escapedArgs = runArgs.Select(arg =>
                {
                    arg = arg
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);

                    return arg.Contains(';') || arg.Contains('&') || arg.Contains('|') || arg.Contains(' ')
                        ? $"\"{arg}\""
                        : arg;
                });

                if (runArgs.Length > 0 && project is { Length: > 0 })
                {
                    project = project
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);

                    if (project.Contains(';') || project.Contains('&') || project.Contains('|') || project.Contains(' '))
                        project = $"\"{project}\"";
                }

                var atomProjectPath = Path.Combine(currentDirectory.FullName, project, $"{project}.csproj");
                var allArgs = new[] { "run", "--project", atomProjectPath, "--" }.Concat(escapedArgs);

                var atomProcess = Process.Start("dotnet", allArgs);
                await atomProcess.WaitForExitAsync(cancellationToken);

                return atomProcess.ExitCode;
            }

            currentDirectory = currentDirectory.Parent;
        }

        Console.WriteLine($"Could not find project '{project}'");
        Console.WriteLine("The project option must be the name of an atom project in the current directory or a parent directory.");

        return 1;
    }
}
