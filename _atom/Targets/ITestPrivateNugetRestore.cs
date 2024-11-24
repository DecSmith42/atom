namespace Atom.Targets;

[TargetDefinition]
internal partial interface ITestPrivateNugetRestore
{
    public const string PrivateTestLibTesterProjectName = "PrivateTestLibTester";

    IProcessRunner ProcessRunner => Services.GetRequiredService<IProcessRunner>();

    Target TestPrivateNugetRestore =>
        d => d.Executes(async () =>
        {
            var runResult = await ProcessRunner.RunAsync(new("dotnet", $"run --project {PrivateTestLibTesterProjectName}"));

            if (runResult.ExitCode is not 0)
                throw new($"Failed to run {PrivateTestLibTesterProjectName} package. Exit code: {runResult.ExitCode}");
        });
}
