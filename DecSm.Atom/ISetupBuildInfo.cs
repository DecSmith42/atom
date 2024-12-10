namespace DecSm.Atom;

[TargetDefinition]
public partial interface ISetupBuildInfo : IBuildInfo
{
    IBuildTimestampProvider BuildIdProvider => GetService<IBuildTimestampProvider>();

    IBuildVersionProvider BuildVersionProvider => GetService<IBuildVersionProvider>();

    IBuildTimestampProvider BuildTimestampProvider => GetService<IBuildTimestampProvider>();

    Target SetupBuildInfo =>
        d => d
            .WithDescription("Sets up the build ID, version, and timestamp")
            .IsHidden()
            .RequiresParam(AtomBuildName)
            .ProducesVariable(nameof(BuildId))
            .ProducesVariable(nameof(BuildVersion))
            .ProducesVariable(nameof(BuildTimestamp))
            .Executes(async () =>
            {
                var buildId = BuildIdProvider.Timestamp;
                await WriteVariable(nameof(BuildId), buildId.ToString());

                var buildVersion = BuildVersionProvider.Version;
                await WriteVariable(nameof(BuildVersion), buildVersion);

                var buildTimestamp = BuildTimestampProvider.Timestamp;
                await WriteVariable(nameof(BuildTimestamp), buildTimestamp.ToString());

                var reportedBuildId = (buildId.ToString() == buildVersion)
                    ? buildId.ToString()
                    : $"{buildVersion} - {buildId} [{buildTimestamp}]";

                AddReportData(new TextReportData($"{AtomBuildName} | {reportedBuildId}")
                {
                    Title = "Run Information",
                    BeforeStandardData = true,
                });

                var logger = Services.GetRequiredService<ILogger<ISetupBuildInfo>>();

                logger.LogInformation("Build ID: {BuildId}", buildId);
                logger.LogInformation("Build Version: {BuildVersion}", buildVersion);
                logger.LogInformation("Build Timestamp: {BuildTimestamp}", buildTimestamp);
            });
}
