using System.Diagnostics;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

while (currentDirectory?.Exists is true)
{
    if (Directory.Exists(Path.Combine(currentDirectory.FullName, "_atom")))
    {
        // Sanitize arguments
        var escapedArgs = args.Select(arg =>
        {
            arg = arg
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            return arg.Contains(';') || arg.Contains('&') || arg.Contains('|') || arg.Contains(' ')
                ? $"\"{arg}\""
                : arg;
        });

        // If an arg is --project or -p, use it (with following arg as value) as project path
        var atomProjectName = "_atom";

        for (var i = 0; i < args.Length; i++)
            if (args[i]
                    .Equals("--project", StringComparison.OrdinalIgnoreCase) ||
                (args[i]
                     .Equals("-p", StringComparison.OrdinalIgnoreCase) &&
                 i < args.Length - 1))
            {
                atomProjectName = args[i + 1]
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
        atomProcess.WaitForExit();

        return atomProcess.ExitCode;
    }

    currentDirectory = currentDirectory.Parent;
}

Console.WriteLine("No Atom project found.");

return 1;
