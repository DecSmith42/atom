﻿namespace DecSm.Atom.Process;

public interface IProcessRunner
{
    string RunProcess(string name, string args, string? workingDirectory = null, bool logInvocation = true, bool suppressError = false);

    string RunProcess(string name, string[] args, string? workingDirectory = null, bool logInvocation = true, bool suppressError = false);

    Task<string> RunProcessAsync(
        string name,
        string args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false);

    Task<string> RunProcessAsync(
        string name,
        string[] args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false);
}

internal sealed class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    public string RunProcess(
        string name,
        string args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new(name)
            {
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        var output = new StringBuilder();

        try
        {
            process.OutputDataReceived += LogOutput;
            process.OutputDataReceived += WriteOutput;
            process.ErrorDataReceived += LogError;

            if (logInvocation)
                logger.LogInformation("Invoking process {Name} {Args}", name, args);

            using (logger.BeginScope(new Dictionary<string, object>
                   {
                       ["$ProcessOutput"] = true,
                   }))
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                logger.LogInformation("");
            }

            if (process.ExitCode != 0 && !suppressError)
                throw new InvalidOperationException($"Process '{name}' exited with code {process.ExitCode}");

            logger.LogInformation("Process '{Name}' completed successfully", name);
        }
        finally
        {
            process.OutputDataReceived -= LogOutput;
            process.OutputDataReceived -= WriteOutput;
            process.ErrorDataReceived -= LogError;
        }

        return output.ToString();

        void WriteOutput(object _, DataReceivedEventArgs e)
        {
            output.AppendLine(e.Data);
        }
    }

    public string RunProcess(
        string name,
        string[] args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false) =>
        RunProcess(name, string.Join(' ', args), workingDirectory, logInvocation, suppressError);

    public async Task<string> RunProcessAsync(
        string name,
        string args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new(name)
            {
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        var output = new StringBuilder();

        try
        {
            process.OutputDataReceived += LogOutput;
            process.OutputDataReceived += WriteOutput;
            process.ErrorDataReceived += LogError;

            if (logInvocation)
                logger.LogInformation("Invoking process {Name} {Args}", name, args);

            using (logger.BeginScope(new Dictionary<string, object>
                   {
                       ["$ProcessOutput"] = true,
                   }))
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                logger.LogInformation("");
            }

            if (process.ExitCode != 0 && !suppressError)
                throw new InvalidOperationException($"Process '{name}' exited with code {process.ExitCode}");

            logger.LogInformation("Process '{Name}' completed successfully", name);
        }
        finally
        {
            process.OutputDataReceived -= LogOutput;
            process.OutputDataReceived -= WriteOutput;
            process.ErrorDataReceived -= LogError;
        }

        return output.ToString();

        void WriteOutput(object _, DataReceivedEventArgs e)
        {
            output.AppendLine(e.Data);
        }
    }

    public async Task<string> RunProcessAsync(
        string name,
        string[] args,
        string? workingDirectory = null,
        bool logInvocation = true,
        bool suppressError = false) =>
        await RunProcessAsync(name, string.Join(' ', args), workingDirectory, logInvocation, suppressError);

    private void LogOutput(object _, DataReceivedEventArgs e) =>
        logger.LogInformation("{Log}", e.Data);

    private void LogError(object _, DataReceivedEventArgs e) =>
        logger.LogError("{Log}", e.Data);
}