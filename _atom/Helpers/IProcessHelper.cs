namespace Atom.Helpers;

[TargetDefinition]
public partial interface IProcessHelper
{
    async Task RunProcess(string name, string args, string? workingDirectory = null, bool logInvocation = true)
    {
        var process = new Process
        {
            StartInfo = new(name)
            {
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        try
        {
            process.OutputDataReceived += LogOutput;
            process.ErrorDataReceived += LogError;

            if (logInvocation)
                Logger.LogInformation("Invoking process {Name} {Args}", name, args);

            using (Logger.BeginScope(new Dictionary<string, object>
                   {
                       ["$ProcessOutput"] = true,
                   }))
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                Logger.LogInformation("");
            }

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Process '{name}' exited with code {process.ExitCode}");

            Logger.LogInformation("Process '{Name}' completed successfully", name);
        }
        finally
        {
            process.OutputDataReceived -= LogOutput;
            process.ErrorDataReceived -= LogError;
        }
    }

    private void LogOutput(object _, DataReceivedEventArgs e) =>
        Logger.LogInformation("{Log}", e.Data);

    private void LogError(object _, DataReceivedEventArgs e) =>
        Logger.LogError("{Log}", e.Data);
}