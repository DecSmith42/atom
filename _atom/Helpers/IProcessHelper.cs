namespace Atom.Helpers;

[TargetDefinition]
public partial interface IProcessHelper
{
    async Task RunProcess(string name, string args, string? workingDirectory = null)
    {
        using var process = Process.Start(new ProcessStartInfo(name)
        {
            Arguments = args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        
        await (process?.WaitForExitAsync() ?? Task.CompletedTask);
        
        var processOutput = await (process?.StandardOutput.ReadToEndAsync() ?? Task.FromResult(string.Empty));
        
        if (process?.ExitCode != 0)
        {
            var processError = await (process?.StandardError.ReadToEndAsync() ?? Task.FromResult(string.Empty));
            Logger.LogError("Failed to run process {Name}\n{Output}\n{Error}", name, processOutput, processError);
            
            throw new InvalidOperationException($"Failed to run process {name}");
        }
        
        Logger.LogInformation("Process {Name} completed successfully\n{Output}", name, processOutput);
    }
}