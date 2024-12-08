namespace DecSm.Atom;

[TargetDefinition]
public partial interface ISetupBuildInfo
{
    [ParamDefinition("atom-build-id", "Build/run ID")]
    string AtomBuildId => GetParam(() => AtomBuildId)!;

    [ParamDefinition("atom-build-version", "Build version")]
    string AtomBuildVersion => GetParam(() => AtomBuildVersion)!;

    IBuildIdProvider BuildIdProvider => GetService<IBuildIdProvider>();

    IBuildVersionProvider BuildVersionProvider => GetService<IBuildVersionProvider>();

    Target SetupBuildInfo =>
        d => d
            .WithDescription("Sets up the build ID and version")
            .IsHidden()
            .RequiresParam(AtomBuildName)
            .ProducesVariable(nameof(AtomBuildId))
            .ProducesVariable(nameof(AtomBuildVersion))
            .Executes(async () =>
            {
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
                    .GetRequiredService<ILogger<ISetupBuildInfo>>()
                    .LogInformation("Build ID: {BuildId}", buildId);

                Services
                    .GetRequiredService<ILogger<ISetupBuildInfo>>()
                    .LogInformation("Build Version: {BuildVersion}", buildVersion);
            });
}
