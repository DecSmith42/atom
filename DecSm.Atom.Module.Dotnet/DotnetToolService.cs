﻿namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface IDotnetToolHelper
{
    void InstallTool(string toolName, string? version = null, bool global = true, bool forceReinstall = false)
    {
        var globalFlag = global
            ? "-g"
            : string.Empty;

        if (!global && !FileSystem.File.Exists(FileSystem.CurrentDirectory / ".config" / "dotnet-tools.json"))
            GetService<ProcessRunner>()
                .Run(new("dotnet", "new tool-manifest")
                {
                    InvocationLogLevel = LogLevel.Debug,
                });

        if (!forceReinstall &&
            !GetService<CommandLineArgs>()
                .HasHeadless)
        {
            var globalListResult = GetService<ProcessRunner>()
                .Run(new("dotnet", $"tool list {toolName} {globalFlag}")
                {
                    AllowFailedResult = true,
                    InvocationLogLevel = LogLevel.Debug,
                });

            var toolVersion = globalListResult
                .Output
                .Split(Environment.NewLine)
                .FirstOrDefault(x =>
                    x.StartsWith(toolName, StringComparison.OrdinalIgnoreCase) &&
                    (version is null or "" || x.Contains(version, StringComparison.OrdinalIgnoreCase)));

            if (toolVersion != null)
            {
                Logger.LogDebug("Tool {ToolName} {Version} is already installed", toolName, version);

                return;
            }
        }

        var versionFlag = version != null
            ? $"-v {version}"
            : string.Empty;

        GetService<ProcessRunner>()
            .Run(new("dotnet", $"tool update {toolName} {versionFlag} {globalFlag}")
            {
                InvocationLogLevel = LogLevel.Debug,
            });
    }

    async Task InstallToolAsync(string toolName, string? version = null, bool global = true, bool forceReinstall = false)
    {
        var globalFlag = global
            ? "-g"
            : string.Empty;

        if (!global && !FileSystem.File.Exists(FileSystem.CurrentDirectory / ".config" / "dotnet-tools.json"))
            await GetService<ProcessRunner>()
                .RunAsync(new("dotnet", "new tool-manifest")
                {
                    InvocationLogLevel = LogLevel.Debug,
                });

        if (!forceReinstall &&
            !GetService<CommandLineArgs>()
                .HasHeadless)
        {
            var globalListResult = await GetService<ProcessRunner>()
                .RunAsync(new("dotnet", $"tool list {toolName} {globalFlag}")
                {
                    AllowFailedResult = true,
                    InvocationLogLevel = LogLevel.Debug,
                });

            var toolVersion = globalListResult
                .Output
                .Split(Environment.NewLine)
                .FirstOrDefault(x =>
                    x.StartsWith(toolName, StringComparison.OrdinalIgnoreCase) &&
                    (version is null or "" || x.Contains(version, StringComparison.OrdinalIgnoreCase)));

            if (toolVersion != null)
            {
                Logger.LogInformation("Tool {ToolName} {Version} is already installed", toolName, version);

                return;
            }
        }

        var versionFlag = version != null
            ? $"-v {version}"
            : string.Empty;

        await GetService<ProcessRunner>()
            .RunAsync(new("dotnet", $"tool update {toolName} {versionFlag} {globalFlag}")
            {
                InvocationLogLevel = LogLevel.Debug,
            });
    }
}
