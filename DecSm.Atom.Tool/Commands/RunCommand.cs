namespace DecSm.Atom.Tool.Commands;

/// <summary>
///     Handles the execution of a DecSm.Atom build project.
/// </summary>
/// <remarks>
///     This command locates the specified Atom project in the current directory or its parent directories,
///     then executes it using `dotnet run`. It sanitizes arguments to prevent shell injection.
/// </remarks>
internal static class RunCommand
{
    /// <summary>
    ///     Executes the specified DecSm.Atom project.
    /// </summary>
    /// <param name="runArgs">Arguments to pass directly to the DecSm.Atom project.</param>
    /// <param name="project">The name of the DecSm.Atom project to run (e.g., "_atom").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The exit code of the executed `dotnet run` command.</returns>
    public static async Task<int> Handle(string[] runArgs, string project, CancellationToken cancellationToken)
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Traverse up the directory tree to find the specified Atom project
        while (currentDirectory?.Exists is true)
        {
            if (Directory.Exists(Path.Combine(currentDirectory.FullName, project)))
            {
                // Sanitize arguments to prevent shell injection
                var escapedArgs = runArgs.Select(arg =>
                {
                    arg = arg
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);

                    // Quote arguments containing special characters
                    return arg.Contains(';') || arg.Contains('&') || arg.Contains('|') || arg.Contains(' ')
                        ? $"\"{arg}\""
                        : arg;
                });

                // Sanitize the project name if it contains special characters
                if (runArgs.Length > 0 && project is { Length: > 0 })
                {
                    project = project
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty);

                    if (project.Contains(';') ||
                        project.Contains('&') ||
                        project.Contains('|') ||
                        project.Contains(' '))
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

        await Console.Error.WriteLineAsync(
            $"Error: Could not find project '{project}' in the current directory or any parent directory.");

        await Console.Error.WriteLineAsync(
            "The project option must be the name of an Atom project (e.g., a directory containing a .csproj file) in the current directory or a parent directory.");

        return 1;
    }
}
