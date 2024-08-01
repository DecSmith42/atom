namespace DecSm.Atom.Process;

[PublicAPI]
public interface IProcessRunner
{
    ProcessRunResult Run(ProcessRunOptions options);

    Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default);
}

internal sealed class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    public ProcessRunResult Run(ProcessRunOptions options)
    {
        if (options.WorkingDirectory is { Length: > 0 })
            logger.Log(options.InvocationLogLevel,
                "Run: {Name} {Args} in {WorkingDirectory}",
                options.Name,
                options.Args,
                options.WorkingDirectory);
        else
            logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.OutputLogLevel < LogLevel.Information)
            logger.LogInformation("{Output}", output);

        if (options.ErrorLogLevel < LogLevel.Information)
            logger.LogWarning("{Error}", error);

        if (options.AllowFailedResult)
        {
            logger.Log(options.InvocationLogLevel, "Process finished with exit code {ExitCode}", result.ExitCode);

            return result;
        }

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            errorBuilder.AppendLine(e.Data);
            logger.Log(options.ErrorLogLevel, "{Error}", e.Data);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            outputBuilder.AppendLine(e.Data);
            logger.Log(options.OutputLogLevel, "{Output}", e.Data);
        }
    }

    public async Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default)
    {
        if (options.WorkingDirectory is { Length: > 0 })
            logger.Log(options.InvocationLogLevel,
                "Run: {Name} {Args} in {WorkingDirectory}",
                options.Name,
                options.Args,
                options.WorkingDirectory);
        else
            logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.AllowFailedResult)
        {
            logger.LogError("Process {Name} {Args} finished with exit code {ExitCode}", options.Name, options.Args, result.ExitCode);

            return result;
        }

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            errorBuilder.AppendLine(e.Data);
            logger.Log(options.ErrorLogLevel, "{Error}", e.Data);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            outputBuilder.AppendLine(e.Data);
            logger.Log(options.OutputLogLevel, "{Output}", e.Data);
        }
    }
}