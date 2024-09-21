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

                var variablesLines = variables
                    .Cast<DictionaryEntry>()
                    .Select(variable => $"{variable.Key}: {variable.Value}");

                Logger.LogInformation("Environment variables: {Variables}", string.Join(Environment.NewLine, variablesLines));
                Logger.LogInformation("BuildIdProvider: {BuildIdProvider}", BuildIdProvider);
                Logger.LogInformation("BuildId: {BuildId}", BuildIdProvider.BuildId);

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
