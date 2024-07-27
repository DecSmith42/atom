namespace DecSm.Atom;

[TargetDefinition]
public interface ISetup : IBuildDefinition
{
    [ParamDefinition("atom-build-id", "Unique build ID")]
    string AtomBuildId => GetParam(() => AtomBuildId)!;

    IBuildIdProvider BuildIdProvider => Services.GetRequiredService<IBuildIdProvider>();

    Target Setup =>
        d => d
            .ProducesVariable(nameof(AtomBuildId))
            .Executes(async () =>
            {
                var buildId = BuildIdProvider.BuildId;

                await WriteVariable(nameof(AtomBuildId), buildId);

                var solutionName = Services
                    .GetRequiredService<IFileSystem>()
                    .SolutionName();

                AddReportData(new TextReportData($"{solutionName} | {buildId}")
                {
                    Title = "Run Information",
                    BeforeStandardData = true,
                });

                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Build ID: {BuildId}", buildId);
            });
}