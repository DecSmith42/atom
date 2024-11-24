using System.Diagnostics;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

while (currentDirectory?.Exists is true)
{
    if (Directory.Exists(Path.Combine(currentDirectory.FullName, "_atom")))
    {
        var atomProjectPath = Path.Combine(currentDirectory.FullName, "_atom", "_atom.csproj");

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

        var allArgs = new[] { "run", "--project", atomProjectPath }.Concat(escapedArgs);

        var atomProcess = Process.Start("dotnet", allArgs);
        atomProcess.WaitForExit();

        return atomProcess.ExitCode;
    }

    currentDirectory = currentDirectory.Parent;
}

Console.WriteLine("No Atom project found.");

return 1;
