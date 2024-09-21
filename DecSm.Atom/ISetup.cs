namespace DecSm.Atom;

[TargetDefinition]
public partial interface ISetup
{
    [ParamDefinition("atom-build-id", "Build/run ID")]
    string AtomBuildId => GetParam(() => AtomBuildId)!;

    [ParamDefinition("atom-build-version", "Build version")]
    string AtomBuildVersion => GetParam(() => AtomBuildVersion)!;

    IBuildIdProvider BuildIdProvider => GetService<IBuildIdProvider>();

    IBuildVersionProvider BuildVersionProvider => GetService<IBuildVersionProvider>();

    Target Setup =>
        d => d
            .WithDescription("Sets up the build")
            .IsHidden()
            .RequiresParam(AtomBuildName)
            .ProducesVariable(nameof(AtomBuildId))
            .ProducesVariable(nameof(AtomBuildVersion))
            .Executes(async () =>
            {
                var variables = Environment.GetEnvironmentVariables();

                foreach (DictionaryEntry variable in variables)
                    Services
                        .GetRequiredService<ILogger<ISetup>>()
                        .LogInformation("{Key}: {Value}", variable.Key, variable.Value);

                var buildId = BuildIdProvider.BuildId ?? throw new StepFailedException("A build ID must be provided");
                await WriteVariable(nameof(AtomBuildId), buildId);

                var buildVersion = BuildVersionProvider.Version;
                await WriteVariable(nameof(AtomBuildVersion), buildVersion);

                var reportedBuildId = buildId == buildVersion
                    ? buildId
                    : $"{buildVersion} - {buildId}";

                AddReportData(new TextReportData($"{AtomBuildName} | {reportedBuildId}")
                {
                    Title = "Run Information",
                    BeforeStandardData = true,
                });

                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Build ID: {BuildId}", buildId);

                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Build Version: {BuildVersion}", buildVersion);
            });
}
