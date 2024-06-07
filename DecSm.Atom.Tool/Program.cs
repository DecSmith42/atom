using System.Diagnostics;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

while (currentDirectory?.Exists is true)
{
    if (Directory.Exists(Path.Combine(currentDirectory.FullName, "_atom")))
    {
        var atomProjectPath = Path.Combine(currentDirectory.FullName, "_atom", "_atom.csproj");
        var atomProcess = Process.Start("dotnet", $"run --project \"{atomProjectPath}\" {string.Join(" ", args)}");
        atomProcess.WaitForExit();
        
        return atomProcess.ExitCode;
    }
    
    currentDirectory = currentDirectory.Parent;
}

Console.WriteLine("No Atom project found.");

return 1;