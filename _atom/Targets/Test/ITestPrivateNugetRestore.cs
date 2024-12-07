namespace Atom.Targets.Test;

[TargetDefinition]
internal partial interface ITestPrivateNugetRestore
{
    public const string PrivateTestLibTesterProjectName = "PrivateTestLibTester";

    ProcessRunner ProcessRunner => Services.GetRequiredService<ProcessRunner>();

    Target TestPrivateNugetRestore =>
        d => d
            .WithDescription("Restores the PrivateTestLibTester nuget package.")
            .Executes(async () =>
            {
                var runResult = await ProcessRunner.RunAsync(new("dotnet",
                    $"run --project {FileSystem.AtomRootDirectory}/{PrivateTestLibTesterProjectName}/{PrivateTestLibTesterProjectName}.csproj"));

                if (runResult.ExitCode is not 0)
                    throw new($"Failed to run {PrivateTestLibTesterProjectName} package. Exit code: {runResult.ExitCode}");
            });
}
